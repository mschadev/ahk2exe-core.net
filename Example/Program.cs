using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ahk2Exe_core_Net;
using Ahk2Exe_core_Net.Utils;
namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            string ahkPath = null;
            string icoPath = null;
            string destPath = null;
            if (args.Length < 1)
            {
                Console.WriteLine("최소 1개 이상의 인수가 필요합니다.");
                return;
            }

            if (args[0].Equals("/?") || args[0].Equals("/help"))
            {
                Console.WriteLine("-a : 스크립트 경로 (필수)" + Environment.NewLine +
                    "-i : 아이콘 경로 (생략 가능)" + Environment.NewLine +
                    "-d : 목적지 경로 (생략 가능)" + Environment.NewLine +
                    "/? or /help : 도움말" + Environment.NewLine +
                    @"예) -a C:\script.ahk -i C:\ico.ico");
                return;
            }
            if (args.Length / 2 == 0)
            {
                Console.WriteLine("인수 갯수가 올바르지 않습니다.");
                return;
            }

            for (int i = 0; i < args.Length; i += 2)
            {

                switch (args[i])
                {

                    case "-i":
                        icoPath = args[i + 1];
                        break;
                    case "-a":
                        ahkPath = args[i + 1];
                        break;
                    case "-d":
                        destPath = args[i + 1];
                        break;
                    default:
                        Console.Write("알수없는 인수 입니다.: " + args[i] + " " + args[i + 1]);
                        return;
                }
            }
            if (ahkPath == null)
            {
                Console.WriteLine("필수 입력인 스크립트 인수가 없습니다.");
                return;
            }
            Core core = new Core(ahkPath, icoPath, destPath);

            CResult Result = core.Compile();
            Console.WriteLine(Util.ToString(Result));
            if (icoPath != null)
            {
                CiconResult iconResult = core.ChangeIco();
                Console.WriteLine(Util.ToString(iconResult));
            }


        }
    }
}
