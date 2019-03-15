using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahk2Exe_core_Net.Utils
{
    public class Util
    {
        public static string ToString(CResult p_result)
        {
            string result = null;
            switch (p_result)
            {
                case CResult.Failed:
                    result = "컴파일을 실패했습니다.";
                    break;
                case CResult.FileInstallFailed:
                    result = "FileInstall 구문 적용에 실패했습니다.";
                    break;
                case CResult.NotExistBinFile:
                    result = "지정된 경로에 바이너리 파일이 없습니다.";
                    break;
                case CResult.NotExistScriptFile:
                    result = "지정된 경로에 스크립트 파일이 없습니다.";
                    break;
                case CResult.Success:
                    result = "컴파일 성공";
                    break;

            }
            return result;
        }
        public static string ToString(CiconResult p_result)
        {
            string result = null;
            switch (p_result)
            {
                case CiconResult.FailBegin:
                    result = "컴파일된 파일 오픈에 실패했습니다.";
                    break;
                case CiconResult.Failed:
                    result = "컴파일된 파일 아이콘 업로드 적용에 실패했습니다.";
                    break;
                case CiconResult.FailUpdate:
                    result = "컴파일된 파일 아이콘 업로드에 실패했습니다.";
                    break;
                case CiconResult.NotExistBinFile:
                    result = "지정된 경로에 컴파일된 파일이 없습니다.";
                    break;
                case CiconResult.NotExistImageFile:
                    result = "지정된 경로에 아이콘 파일이 없습니다.";
                    break;
                case CiconResult.Success:
                    result = "아이콘 변경 완료";
                    break;
            }
            return result;
        }
    }
}
