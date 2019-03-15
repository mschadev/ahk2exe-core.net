using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using ImageMagick;
using System.Threading;
using Ahk2Exe_core_Net.Utils;
using System.Reflection;
namespace Ahk2Exe_core_Net
{
    /// <summary>
    /// 오토핫키 컴파일러 코어
    /// </summary>
    public class Core
    {

        private string _icoPath = null; //ICO파일 경로
        private string _scriptPath = null; //스크립트 파일 경로
        private string _scriptCode = null; //스크립트 코드(전처리기 거친 상태.)
        private string _fileDestPath = null; //컴파일후 목적지
        private string _tempPath = null; //임시 폴더 경로
        private string _binaryPath = null; //바이너리 파일 경로
        public string Script
        {
            get
            {
                return this._scriptPath;
            }
        }
        public string Ico
        {
            get
            {
                return this._icoPath;
            }
        }
        
        public string ScriptCode
        {
            get
            {
                return this._scriptCode;
            }
        }
        public string FileDest
        {
            get
            {
                return this._fileDestPath;
            }
        }
        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="p_ahkPath">스크립트 파일 경로</param>
        /// <param name="p_icoPath">ICO 파일 경로</param>
        /// <param name="p_destPath">목적지 경로</param>
        public Core(string p_ahkPath, string p_icoPath = null, string p_destPath = null,string p_binaryName = null)
        {
            _scriptPath = p_ahkPath;
            _icoPath = p_icoPath;
            _fileDestPath = p_destPath;
            _binaryPath = p_binaryName;
            
            string Ahk = null;
            List<string> ExtraFiles = new List<string>();
            Preprocessor.PreProcess(ref Ahk, _scriptPath, ref ExtraFiles, new List<string>());
            _scriptCode = Ahk;
            _tempPath = System.IO.Directory.GetCurrentDirectory() + @"\temp";
            if (!Directory.Exists(_tempPath))
            {
                Directory.CreateDirectory(_tempPath);
            }
            if(_binaryPath == null)
            {
                byte[] file = Properties.Resources.AutoHotkeySC;
                File.WriteAllBytes(_tempPath + @"\AutoHotkeySC.bin",file);
                _binaryPath = _tempPath + @"\AutoHotkeySC.bin";
            }
        }
        /// <summary>
        /// 소멸자
        /// </summary>
        ~Core()
        {
            if (Directory.Exists(_tempPath)) 
            {
                Directory.Delete(_tempPath, true); //임시 폴더 삭제
            }
        }
        /// <summary>
        /// 아이콘 체인지 메서드
        /// </summary>
        public CiconResult ChangeIco()
        {
            IconChanger Ic = new IconChanger();
            string icoPath = "";
            if (!Path.GetExtension(_icoPath).Equals(".ico"))
            {
                using (MagickImage image = new MagickImage(_icoPath))
                {
                    image.Settings.SetDefine("icon:auto-resize", "256,128,96,64,48,32,16"); //TODO:https://www.imagemagick.org/discourse-server/viewtopic.php?t=14080
                    image.Write(_tempPath + @"\tempico.ico");
                    image.Dispose();
                    
                }
                icoPath = _tempPath + @"\tempico.ico";   
            }
            else
            {
                icoPath = _icoPath;
            }
            if (_fileDestPath == null) //exe파일 저장 경로를 지정 안했을때
            {
                return Ic.ChangeIcon(Path.GetDirectoryName(_scriptPath) + @"\" + Path.GetFileNameWithoutExtension(_scriptPath) + ".exe", icoPath);
            }
            else
            {
                return Ic.ChangeIcon(Path.GetDirectoryName(_fileDestPath) + @"\" + Path.GetFileNameWithoutExtension(_fileDestPath) + ".exe", icoPath);
            }
        }
        /// <summary>
        /// 컴파일 메서드
        /// </summary>
        /// <returns>컴파일 작업후 결과값</returns>
        public CResult Compile()
        {

            if (!File.Exists(_binaryPath))
            {
                return CResult.NotExistBinFile; //Bin파일이 없을때
            }
            if (!File.Exists(_scriptPath))
            {
                return CResult.NotExistScriptFile; //ahk파일이 없을때
            }
            string temp = null; //작업 임시 경로
            string fileName = null; //파일 이름
            if (_fileDestPath == null)
            {
                //목적지가 없다면
                temp = Path.GetDirectoryName(_scriptPath);
                fileName = Path.GetFileNameWithoutExtension(_scriptPath) + ".exe";
            }
            else
            {
                temp = Path.GetDirectoryName(_fileDestPath);
                fileName = Path.GetFileNameWithoutExtension(_fileDestPath) + ".exe";
            }
            if (File.Exists(temp + @"\" + fileName))
            {
                File.Delete(temp + @"\" + fileName);
            }
            try
            {
                string Ahk = null; //전처리기 결과값을 받을 변수
                List<string> ExtraFiles = new List<string>(); //전처리기 분석결과 리소스에 올려야 할 파일들
                Preprocessor.PreProcess(ref Ahk, _scriptPath, ref ExtraFiles, new List<string>());
                _scriptCode = Ahk;
                File.Copy(_binaryPath, temp + @"\" + fileName); //목적지에 복사
                IntPtr handle = WinAPI.BeginUpdateResource(temp + @"\" + fileName, false);
                bool res = WinAPI.UpdateResource(handle, 10, Encoding.UTF8.GetBytes(">AUTOHOTKEY SCRIPT<"), 0x409, Encoding.UTF8.GetBytes(_scriptCode),Convert.ToUInt32(Encoding.UTF8.GetBytes(_scriptCode).Length));
                if (res)
                {
                    WinAPI.EndUpdateResource(handle, false);
                }
                
                if(!FileUploadResource(temp + @"\" + fileName, ExtraFiles)) //리소스 업로드에 실패했을때
                {
                    return CResult.FileInstallFailed;
                }
                
                
            }
            catch (Exception)
            {
                return CResult.Failed;
            }
            return CResult.Success;

        }
        /// <summary>
        /// FileInstall 명령어에 필요한 파일들을 업로드 해주는 함수
        /// </summary>
        /// <param name="p_targetExe">파일을 업로드할 바이너리(.EXE)파일</param>
        /// <param name="p_fileList">업로드할 파일 리스트</param>
        /// <returns>성공 or 실패 여부</returns>
        public bool FileUploadResource(string p_targetExe,List<string> p_fileList)
        {
            if (p_fileList.Count == 0)
            {
                return true;
            }
            if (!File.Exists(p_targetExe))
            {
                return false;
            }
            IntPtr handle = WinAPI.BeginUpdateResource(p_targetExe, false);
            for(int i = 0; i < p_fileList.Count; i++)
            {

                bool res = WinAPI.UpdateResource(handle, 10, Encoding.UTF8.GetBytes(p_fileList[i].ToUpper()), 0x409, Encoding.UTF8.GetBytes(File.ReadAllText(p_fileList[i])), Convert.ToUInt32(Encoding.UTF8.GetBytes(File.ReadAllText(p_fileList[i])).Length));
                if (!res)
                {
                    WinAPI.EndUpdateResource(handle, false);
                    return false;
                }
            }
            WinAPI.EndUpdateResource(handle, false);
            return true;
        }
    }
}
