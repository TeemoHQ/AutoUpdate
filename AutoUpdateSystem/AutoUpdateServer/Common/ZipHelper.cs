using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Common
{
    public class ZipHelper
    {
        public static bool Zip(string path, string zipName)
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT ? WindowsZip(path, zipName) : LinuxZip(path, zipName);
        }

        public static bool UnZip(string zipFileName, string unZipPath)
        {
            LogHelper.instance.Logger.Info($"进入UnZip;");
            return Environment.OSVersion.Platform == PlatformID.Win32NT ? WindowsUnZip(zipFileName, unZipPath) : LinuxUnZip(zipFileName, unZipPath);
        }

        public static bool WindowsZip(string path, string zipName)
        {
            Process winrarPro = new Process();
            winrarPro.StartInfo.WorkingDirectory = path;
            winrarPro.StartInfo.WindowStyle = ProcessWindowStyle.Minimized; //隐藏压缩窗口
            winrarPro.StartInfo.FileName = GetProcessPath();
            winrarPro.StartInfo.CreateNoWindow = false;
            winrarPro.StartInfo.Arguments = @"a ..\" + zipName + " * -r";
            winrarPro.Start();
            winrarPro.WaitForExit();
            int iExitCode = 0;
            if (winrarPro.HasExited)
            {
                iExitCode = winrarPro.ExitCode;
                winrarPro.Close();
                if (iExitCode != 0 && iExitCode != 1)
                {
                    return false;
                }
            }
            return true;
        }
        private static bool WindowsUnZip(string zipFileName, string unZipPath)
        {
            try
            {
                string arguments = " x -y \"" + zipFileName + "\" -o\"" + unZipPath + "\"";
                Process winrarPro = new System.Diagnostics.Process();
                winrarPro.StartInfo.WindowStyle = ProcessWindowStyle.Minimized; //隐藏压缩窗口
                winrarPro.StartInfo.FileName = GetProcessPath();
                winrarPro.StartInfo.CreateNoWindow = false;
                winrarPro.StartInfo.Arguments = arguments;
                winrarPro.Start();
                winrarPro.WaitForExit();
                int iExitCode = 0;
                if (winrarPro.HasExited)
                {
                    iExitCode = winrarPro.ExitCode;
                    winrarPro.Close();
                    if (iExitCode != 0 && iExitCode != 1)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static string GetProcessPath()
        {
            var in64Bit = (IntPtr.Size == 8);
            return in64Bit ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"7ZIP\64bit\7z.exe") :
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"7ZIP\32bit\7z.exe");
        }

        //zip -r test.zip *
        //7za a test.7z * 
        private static bool LinuxZip(string path, string zipName)
        {
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = "sh";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.StandardInput.WriteLine(@"cd \;");
                p.StandardInput.WriteLine($"cd {path};");
                p.StandardInput.WriteLine($"7za a ../{zipName} * ;");
                p.StandardInput.WriteLine("exit;");
                string strResult = p.StandardOutput.ReadToEnd();
                p.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        //7za x [文件名] -o[解压地址]; -y表示如果有子文件，覆盖
        private static bool LinuxUnZip(string zipFileName, string unZipPath)
        {
            try
            {
                LogHelper.instance.Logger.Info($"进入LinuxUnZip;");
                Process p = new Process();
                p.StartInfo.FileName = "sh";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.StandardInput.WriteLine(@"cd \;");
                p.StandardInput.WriteLine($"cd {ConstFile.TempPath};");
                p.StandardInput.WriteLine($"7za x -y {zipFileName} -o{unZipPath};");
                p.StandardInput.WriteLine("exit;");
                string strResult = p.StandardOutput.ReadToEnd();
                p.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


    }
}
