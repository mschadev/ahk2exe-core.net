using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
namespace Ahk2Exe_core_Net
{
    /// <summary>
    /// 전처리기 클래스
    /// </summary>
    
    /*
     * 이 전처리기는 오토핫키 컴파일러의 기능을 대부분 컨버팅 했습니다.
     */
    public class Preprocessor
    {
        private class Options
        {
            private string comm = ";";
            private string esc = @"`";
            public string Comm
            {
                set
                {
                    comm = value;
                }
                get
                {
                    return comm;
                }
            }
            public string Esc
            {
                set
                {
                    esc = value;
                }
                get
                {
                    return esc;
                }
            }
        }
        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="ScriptText">전처리 작업후 담길 변수</param>
        /// <param name="AhkScript">스크립트 파일 경로</param>
        /// <param name="ExtraFiles">리소스에 업로드해야 파일경로들이 담길 변수</param>
        /// <param name="FileList">Include 관련</param>
        /// <param name="FirstScriptDir">처음 작업하는 스크립트 파일 폴더 경로</param>
        /// <param name="IgnoreErrors">?</param>
        public static void PreProcess(ref string ScriptText, string AhkScript,ref List<string> ExtraFiles,List<string> FileList=null,string FirstScriptDir="",bool IgnoreErrors=false)
        {
            if(FileList == null)
            {
                FileList = new List<string>();
            }
            if (!File.Exists(AhkScript))
            {
                return;
            }
           
            string SetWorkingDir = null;
            if (FileList.Count == 0)
            {
                ScriptText += "; <COMPILR: PLOAT> \n";
                FirstScriptDir = Path.GetDirectoryName(AhkScript);
            }
            PreprocessorUtil Su = new PreprocessorUtil();
            Options options = new Options();
            bool cmtBlock = false, contSection = false;
            string[] temp = File.ReadAllLines(AhkScript);
            for (int i = 0; i < temp.Length; i++)
            {
                string tline = temp[i].Trim();
                if (!cmtBlock)
                {
                    if (!contSection)
                    {
                        if (tline.StartsWith(options.Comm))
                        {
                            continue;
                        }
                        else if (tline == "")
                        {
                            continue;
                        }
                        else if (tline.StartsWith("/*"))
                        {
                            cmtBlock = true;
                            continue;
                        }
                    }
                    if (tline.StartsWith("(") && Su.IsFakeCSOpening(tline))
                    {
                        contSection = true;
                    }
                    else if (tline.StartsWith(")"))
                    {
                        contSection = false;
                    }
                    tline = Su.RegExReplace(tline, @"\s+"+ options.Comm+".*");
                    string result = null;
                    if (!contSection && (result = Su.RegExMatch(tline, @"#Include(Again)?[ \t]*[, \t]?\s+(.*)$",true)) != null)
                    {
                        bool IsIncludeAgain = result.Equals("Again") ? true : false;
                        string IncludeFile = null;
                        if (result.Contains("#include"))
                        {
                            IncludeFile = result.Replace("#include ", "");

                        }
                        else
                        {
                            IncludeFile = result.Replace("#Include ", "");
                        }
                        if ((result = Su.RegExMatch(IncludeFile, @"\*[iI]\s+?(.*)")) != null)
                        {
                            IgnoreErrors = true;
                            IncludeFile = result.Trim();
                        }
                        if ((result = Su.RegExMatch(IncludeFile, @"&<(.+)>$")) != null)
                        {
                            string IncFile2 = null;
                            if((IncFile2 = Su.FindLibraryFile(result,FirstScriptDir))!=null)
                            {
                                IncludeFile = IncFile2;
                                goto _skip_findfile;
                            }
                        }
                        IncludeFile = IncludeFile.Replace("`%A_ScriptDir`%", FirstScriptDir);
                        IncludeFile = IncludeFile.Replace("%A_AppData`%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                        IncludeFile = IncludeFile.Replace("`%A_AppDataCommon`%", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
                        if (Directory.Exists(IncludeFile))
                        {
                            SetWorkingDir = IncludeFile;
                        }
                    _skip_findfile:;
                        IncludeFile = FirstScriptDir + @"\"+IncludeFile;

                        bool AlreadyIncluded = false;
                        for(int j = 0; j < FileList.Count; j++)
                        {
                            if (FileList[i].Equals(IncludeFile))
                            {
                                AlreadyIncluded = true;
                                break;
                            }

                        }
                        if(IsIncludeAgain || !AlreadyIncluded)
                        {
                            if (!AlreadyIncluded)
                            {
                                FileList.Add(IncludeFile);
                            }
                            PreProcess(ref ScriptText, IncludeFile,ref ExtraFiles, FileList,FirstScriptDir,IgnoreErrors);
                        }

                    }
                    else if(!contSection && Su.RegExMatch(tline,@"FileInstall[,\t]",true) != null)
                    {
                        if(Su.RegExMatch(tline, @"^\w+\s+(:=|\+=|-=|\*=|\/=|\/\/=|\.=|\|=|&=|\^=|>>=|<<=)") != null)
                        {
                            continue;
                        }
                        if((result = Su.RegExMatch(tline, @"^FileInstall[ \t]*[, \t][ \t]*([^,]+?)[ \t]*(,|$)",true)) != null)
                        {
                            if (Su.RegExMatch(tline, @"[^``]%") != null)
                            {
                                //ERROR!
                            }
                        }
                        result = Su.RegExMatch(result, @"[, \t][\w\W]+[ \t,]");
                        result = result.Replace(options.Esc + @"`%", @"`%");
                        result = result.Replace(options.Esc + @"`,", @"`,");
                        result = result.Replace(@options.Esc + options.Esc + ",", options.Esc+@",");

                        result = result.Trim().Replace(",","");
                        ExtraFiles.Add(result);
                        ScriptText += tline + Environment.NewLine;


                    }
                    else if(!contSection && Su.RegExMatch(tline, @"^#CommentFlag\s+(.+)$",true) != null)
                    {
                        result = Su.RegExMatch(tline, @"\s+(.+)$").Trim();
                        options.Comm = result;
                        ScriptText += tline + Environment.NewLine;
                    }
                    else if(!contSection && Su.RegExMatch(tline, @"^#EscapeChar\s+(.+)$",true) != null)
                    {
                        result = Su.RegExMatch(tline, @"\s+(.+)$").Trim();
                        options.Esc = result;
                        ScriptText += tline + Environment.NewLine;
                    }
                    else
                    {
                        ScriptText += (contSection ? temp[i] : tline)+Environment.NewLine;
                    }

                }
                else if (tline.StartsWith(" */"))
                {
                    cmtBlock = false;
                }
            }


        }
    }
    /// <summary>
    /// 전처리기에 필요한 메서드
    /// </summary>
    class PreprocessorUtil
    {
        public bool IsFakeCSOpening(string tline)
        {
            string[] temp = tline.Split(' ');
            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i].StartsWith("Join") && temp[i].IndexOf(")") != 0)
                {
                    return true;
                }
            }
            return false;
        }
        public string RegExMatch(string source, string pattern,bool IsIgnoreCased = false)
        {
            Regex regex = new Regex(pattern);
            RegexOptions options;
            if (IsIgnoreCased)
            {
                options = RegexOptions.IgnoreCase | RegexOptions.None;
            }
            else
            {
                options = RegexOptions.None;
            }
          

            Match match = Regex.Match(source, pattern, options);

            if (match.Length == 0)
            {
                return null;
            }
            else
            {
                return match.Value;
            }
        }
        public string RegExReplace(string Source, string pattern)
        {
            Regex regex = new Regex(pattern);
            return regex.Replace(Source, "");
        }
        public string FindLibraryFile(string name, string ScriptDir)
        {
            string[] libs = { ScriptDir + @"\Lib",
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\AutoHotkey\Lib",
                Directory.GetCurrentDirectory() + @"\..\Lib"
            };
            int p = name.IndexOf("_");
            string name_libs = null;
            if (p != 0)
            {
                name_libs = name.Substring(1, p - 1);
            }
            
            for (int i = 0; i < libs.Length; i++)
            {
                string file = libs[i] + @"\" + name + ".ahk";
                if (File.Exists(file))
                {
                    return file;
                }
                if (p != 0)
                {
                    continue;
                }
                file = libs[i] + @"\" + name_libs + ".ahk";
                if (File.Exists(file))
                {
                    return file;
                }
            }
            return null;
        }
    }
}
