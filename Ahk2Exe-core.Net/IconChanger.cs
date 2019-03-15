using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using Ahk2Exe_core_Net.Utils;
namespace Ahk2Exe_core_Net
{
    public class IconChanger
    {

        #region IconReader
        public class Icons : List<Icon>
        {
            public byte[] ToGroupData(int startindex = 1)
            {
                using (var ms = new MemoryStream())
                using (var writer = new BinaryWriter(ms))
                {
                    var i = 0;

                    writer.Write((ushort)0);  //reserved, must be 0
                    writer.Write((ushort)1);  // type is 1 for icons
                    writer.Write((ushort)this.Count);  // number of icons in structure(1)

                    foreach (var icon in this)
                    {

                        writer.Write(icon.Width);
                        writer.Write(icon.Height);
                        writer.Write(icon.Colors);

                        writer.Write((byte)0); // reserved, must be 0
                        writer.Write(icon.ColorPlanes);

                        writer.Write(icon.BitsPerPixel);

                        writer.Write(icon.Size);

                        writer.Write((ushort)(startindex + i));

                        i++;

                    }
                    ms.Position = 0;

                    return ms.ToArray();
                }
            }
        }

        public class Icon
        {

            public byte Width { get; set; }
            public byte Height { get; set; }
            public byte Colors { get; set; }

            public uint Size { get; set; }

            public uint Offset { get; set; }

            public ushort ColorPlanes { get; set; }

            public ushort BitsPerPixel { get; set; }

            public byte[] Data { get; set; }

        }

        public class IconReader
        {

            public Icons Icons = new Icons();

            public IconReader(Stream input)
            {
                using (BinaryReader reader = new BinaryReader(input))
                {
                    reader.ReadUInt16(); // ignore. Should be 0
                    var type = reader.ReadUInt16();
                    if (type != 1)
                    {
                        throw new Exception("Invalid type. The stream is not an icon file");
                    }
                    var num_of_images = reader.ReadUInt16();

                    for (var i = 0; i < num_of_images; i++)
                    {
                        var width = reader.ReadByte();
                        var height = reader.ReadByte();
                        var colors = reader.ReadByte();
                        reader.ReadByte(); // ignore. Should be 0

                        var color_planes = reader.ReadUInt16(); // should be 0 or 1

                        var bits_per_pixel = reader.ReadUInt16();

                        var size = reader.ReadUInt32();

                        var offset = reader.ReadUInt32();

                        this.Icons.Add(new Icon()
                        {
                            Colors = colors,
                            Height = height,
                            Width = width,
                            Offset = offset,
                            Size = size,
                            ColorPlanes = color_planes,
                            BitsPerPixel = bits_per_pixel
                        });
                    }

                    // now get the Data
                    foreach (var icon in Icons)
                    {
                        if (reader.BaseStream.Position < icon.Offset)
                        {
                            var dummy_bytes_to_read = (int)(icon.Offset - reader.BaseStream.Position);
                            reader.ReadBytes(dummy_bytes_to_read);
                        }

                        var data = reader.ReadBytes((int)icon.Size);

                        icon.Data = data;
                    }

                }
            }

        }
        #endregion

        const uint RT_ICON = 3;
        const uint RT_GROUP_ICON = 14;

        public CiconResult ChangeIcon(string p_exeFilePath, string p_iconFilePath)
        {
            if (!File.Exists(p_iconFilePath))
            {
                return CiconResult.NotExistImageFile;
            }
            if (!File.Exists(p_exeFilePath))
            {
                return CiconResult.NotExistBinFile;
            }
            using (FileStream fs = new FileStream(p_iconFilePath, FileMode.Open, FileAccess.Read))
            {
                var reader = new IconReader(fs);

                var iconChanger = new IconChanger();
                return iconChanger.ChangeIcon(p_exeFilePath, reader.Icons);
            }
        }

        public CiconResult ChangeIcon(string p_exeFilePath, Icons p_icons)
        {
            // Load executable
            IntPtr handleExe = WinAPI.BeginUpdateResource(p_exeFilePath, false);
            if (handleExe == null)
            {
                return CiconResult.FailBegin;
            }

            ushort startindex = 159;
            ushort index = 1;
            CiconResult result = CiconResult.Success;

            bool ret = true;

            foreach (var icon in p_icons)
            {
                // Replace the icon
                // todo :Improve the return value handling of UpdateResource
                ret = WinAPI.UpdateResource(handleExe, RT_ICON, index, 0x409, icon.Data, icon.Size);

                index++;
            }

            var groupdata = p_icons.ToGroupData();

            // todo :Improve the return value handling of UpdateResource
            ret = WinAPI.UpdateResource(handleExe, RT_GROUP_ICON, startindex, 0x409, groupdata, (uint)groupdata.Length);
            if (ret)
            {
                if (WinAPI.EndUpdateResource(handleExe, false))
                    result = CiconResult.Success;
                else
                    result = CiconResult.Failed;
            }
            else
                result = CiconResult.FailUpdate;

            return result;
        }
    }
}
