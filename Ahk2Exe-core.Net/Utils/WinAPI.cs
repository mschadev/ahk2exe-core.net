using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace Ahk2Exe_core_Net.Utils
{
    /// <summary>
    /// 윈도우 API 모음
    /// </summary>
    class WinAPI
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr BeginUpdateResource(string pFileName,[MarshalAs(UnmanagedType.Bool)]bool bDeleteExistingResources);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool UpdateResource(IntPtr hUpdate, uint lpType, byte[] lpName, ushort wLanguage, byte[] lpData, uint cbData);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool UpdateResource(IntPtr hUpdate, uint lpType, ushort lpName, ushort wLanguage, byte[] lpData, uint cbData);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

    }
}
