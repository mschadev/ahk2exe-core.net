using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahk2Exe_core_Net.Utils
{
    /// <summary>
    /// 컴파일 결과 열거체
    /// </summary>
    public enum CResult
    {
        NotExistBinFile,
        NotExistScriptFile,
        Failed,
        Success,
        FileInstallFailed,
    }
    /// <summary>
    /// 아이콘변경 결과 열거체
    /// </summary>
    public enum CiconResult
    {
        NotExistBinFile,
        NotExistImageFile,
        Failed,
        Success,
        FailBegin,
        FailUpdate,


    }
}
