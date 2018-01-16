using AutoUpdateClient.Common;
using AutoUpdateClient.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Threading;

namespace AutoUpdateClient
{
    public class Monitor
    {
        private static bool _isFree;
        private static bool _isZiping;
        private static string _exePath = string.Empty;
        private static string _exeDir = string.Empty;
        public static Action<string> MonitorDelegate;
        public static Action MinimizedDelegate;
        public static Action MaxmizedDelegate;
        internal static void CheckClientAlive(object state)
        {
            if (!UserConfigInstance.Instance.Config.FirstInstall)
            {
                if (Process.GetProcessesByName(ConstFile.CLIENTEXENAME).Any())
                {
                    MonitorDelegate("【保护线程】 客户端正在运行");
                    CheckClientFree();
                }
                else
                {
                    if (!StateCenter.Instance.HasRepalced || !StateCenter.Instance.HasRunDynCoded)
                    {
                        return;
                    }
                    if (StartClient())
                    {
                        MonitorDelegate("【保护线程】启动客户端成功");
                        CheckClientFree();
                    }
                }
            }
            else
            {
                FirstInstall();
            }
        }
        private static bool StartClient(bool isFirstInstall = false)
        {
            if (!isFirstInstall)
            {
                GetClientExePath(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory));
                if (string.IsNullOrEmpty(_exePath))
                {
                    LogHelper.Info("【保护线程】 找不到客户端程序");
                    MonitorDelegate("【保护线程】 找不到客户端程序");
                    return false;
                }
            }
            else
            {
                _exePath = Path.Combine(ConstFile.DefaultClientDirectory, ConstFile.CLIENTEXENAME + ".exe");
                _exeDir = ConstFile.DefaultClientDirectory;
            }
            Process p = new Process();
            p.StartInfo.FileName = _exePath;
            p.StartInfo.WorkingDirectory = _exeDir;
            return p.Start();
        }
        private static void CheckClientFree()
        {
            if (StateCenter.Instance.HasRepalced && StateCenter.Instance.HasRunDynCoded)
            {
                MinimizedDelegate();
                return;
            }
            if (_isZiping)
            {
                MonitorDelegate("【保护线程】 正在安装客户端");
                return;
            }
            _isZiping = true;
            var res = PipeHelper.Communication("Are you Free?");
            bool.TryParse(res, out _isFree);
            MonitorDelegate($"【保护线程】 检查空闲, 空闲状态：{_isFree}");
            if (!string.IsNullOrEmpty(res) && !_isFree)
            {
                MonitorDelegate("【保护线程】 当前客户端忙碌,不允许更新");
            }
            else
            {
                //关闭客户端，文件替换，重启客户端
                MonitorDelegate("【保护线程】 更新开始 ");
                KillProcess();
                MaxmizedDelegate();
                if (!StateCenter.Instance.HasRepalced)
                {
                    if (!string.IsNullOrEmpty(Modifier.VersionResponse.Data.BlackList) ||
                        !string.IsNullOrEmpty(Modifier.VersionResponse.Data.ExistSoIgnoreList))
                    {
                        if (ZipHelper.UnZipPath(ConstFile.PackagePath, ConstFile.TempPath))
                        {
                            BatchFiles();
                            Finish();
                        }
                        else
                        {
                            MonitorDelegate($"【保护线程】 安装客户端失败 解压失败");
                            return;
                        }
                    }
                    else
                    {
                        if (ZipHelper.UnZipPath(ConstFile.PackagePath, ConstFile.DefaultClientDirectory))
                        {
                            Finish();
                        }
                        else
                        {
                            MonitorDelegate($"【保护线程】 安装客户端失败 解压失败");
                            return;
                        }
                    }
                }
            }
            _isZiping = false;

        }
        private static void FirstInstall()
        {
            if (StateCenter.Instance.HasRepalced)
            {
                MonitorDelegate("【保护线程】 正在等待下载客户端");
                return;
            }
            if (_isZiping)
            {
                MonitorDelegate("【保护线程】 正在安装客户端");
                return;
            }
            _isZiping = true;
            MonitorDelegate("【保护线程】 开始安装客户端");
            KillProcess();
            MaxmizedDelegate();
            if (!Directory.Exists(ConstFile.DefaultClientDirectory))
            {
                Directory.CreateDirectory(ConstFile.DefaultClientDirectory);
            }
            if (!string.IsNullOrEmpty(Modifier.VersionResponse.Data.BlackList) ||
                !string.IsNullOrEmpty(Modifier.VersionResponse.Data.ExistSoIgnoreList))
            {
                if (ZipHelper.UnZipPath(ConstFile.PackagePath, ConstFile.TempPath))
                {
                    BatchFiles();
                }
                else
                {
                    MonitorDelegate($"【保护线程】 安装客户端失败 解压失败");
                    return;
                }
            }
            else
            {
                if (ZipHelper.UnZipPath(ConstFile.PackagePath, ConstFile.DefaultClientDirectory))
                {
                }
                else
                {
                    MonitorDelegate($"【保护线程】 安装客户端失败 解压失败");
                    return;
                }
            }
            Finish();
            _isZiping = false;
        }

        //<KillProcesses>
        //<string>fileServer</string>
        //<string>CameraService</string>
        //</KillProcesses>
        //<!--温和关闭的进程-->
        //<CloseMainWindowProcesses>
        //<string>Terminal</string>
        //</CloseMainWindowProcesses>
        private static void KillProcess()
        {
            Process[] myproc = Process.GetProcesses();
            if (UserConfigInstance.Instance.Config.KillProcesses == null ||
                UserConfigInstance.Instance.Config.KillProcesses.Count <= 0)
            {
                UserConfigInstance.Instance.Config.KillProcesses.Add("fileServer");
                UserConfigInstance.Instance.Config.KillProcesses.Add("CameraService");
            }
            if (UserConfigInstance.Instance.Config.CloseMainWindowProcesses == null ||
                UserConfigInstance.Instance.Config.CloseMainWindowProcesses.Count <= 0)
            {
                UserConfigInstance.Instance.Config.CloseMainWindowProcesses.Add("Terminal");
            }
            foreach (Process item in myproc)
            {
                if (UserConfigInstance.Instance.Config.KillProcesses.Contains(item.ProcessName))
                {
                    item.Kill();
                }
                else if (UserConfigInstance.Instance.Config.CloseMainWindowProcesses.Contains(item.ProcessName))
                {
                    item.CloseMainWindow();
                    item.WaitForExit();
                }
            }

        }
        private static void GetClientExePath(DirectoryInfo dir)
        {
            foreach (var item in dir.GetFiles())
            {
                if (item.Name.ToUpper()==(ConstFile.CLIENTEXENAME+".EXE").ToUpper())
                {
                    ConstFile.DefaultClientDirectory = item.DirectoryName;
                    _exeDir = item.DirectoryName;
                    _exePath = item.FullName;
                    return;
                }
            }
            foreach (var item in dir.GetDirectories())
            {
                GetClientExePath(item);
            }
        }
        private static void BatchFiles()
        {
            if (!Directory.Exists(ConstFile.TempPath))
            {
                return;
            }
            var blackList = new List<string>();
            if (!string.IsNullOrEmpty(Modifier.VersionResponse.Data.BlackList))
            {
                blackList = Modifier.VersionResponse.Data.BlackList.Trim().Split(',').ToList();
            }
            var existSoIgnoreList = new List<string>();
            if (!string.IsNullOrEmpty(Modifier.VersionResponse.Data.ExistSoIgnoreList))
            {
                existSoIgnoreList = Modifier.VersionResponse.Data.ExistSoIgnoreList.Trim().Split(',').ToList();
            }
            var filesPath = new List<string>();
            GetFilesPath(filesPath, ConstFile.TempPath, blackList);
            foreach (var filePath in filesPath)
            {
                var resourceInfo = new FileInfo(filePath);
                var destPath = resourceInfo.FullName.Replace(ConstFile.TEMP, ConstFile.CLIENTEXENAME);
                var destInfo = new FileInfo(destPath);
                //存在则忽略
                if (File.Exists(destPath) && existSoIgnoreList.Count >= 0)
                {
                    var hasFlag = false;
                    foreach (var existSoIgnoreName in existSoIgnoreList)
                    {
                        if (filePath.Contains(existSoIgnoreName))
                        {
                            hasFlag = true;
                        }
                    }
                    if (!hasFlag)
                    {
                        resourceInfo.CopyTo(destPath, true);
                    }
                }
                else
                {
                    if (!Directory.Exists(destInfo.DirectoryName))
                    {
                        Directory.CreateDirectory(destInfo.DirectoryName);
                    }
                    resourceInfo.CopyTo(destPath, true);
                }
            }
        }
        private static void GetFilesPath(List<string> filesPath, string dirctoryPath, List<string> blackList)
        {

            var dirInfo = new DirectoryInfo(dirctoryPath);
            foreach (var file in dirInfo.GetFiles())
            {
                //黑名单排除
                if (blackList.Count <= 0)
                {
                    filesPath.Add(file.FullName);
                    continue;
                }
                var hasFlag = false;
                foreach (var balckName in blackList)
                {
                    if (file.Name.Contains(balckName))
                    {
                        hasFlag = true;
                    }
                }
                if (!hasFlag)
                {
                    filesPath.Add(file.FullName);
                }
            }
            foreach (var dir in dirInfo.GetDirectories())
            {
                GetFilesPath(filesPath, dir.FullName, blackList);
            }
        }
        private static void Finish()
        {
            //处理动态代码
            RunDynCode.GrammarTest(Modifier.VersionResponse.Data.DynamicCode);
            if (File.Exists(ConstFile.PackagePath))
            {
                File.Delete(ConstFile.PackagePath);
            }
            if (Directory.Exists(ConstFile.TempPath))
            {
                Directory.Delete(ConstFile.TempPath, true);
            }
            StateCenter.Instance.HasRepalced = true;
            StateCenter.Instance.HasRunDynCoded = true;
            UserConfigInstance.Instance.Config.FirstInstall = false;
            UserConfigInstance.Instance.Config.TerminalVersion = Modifier.VersionResponse.Data.Version;
            UserConfigInstance.Instance.Config.DynamicCodeVersion = Modifier.VersionResponse.Data.DynamicCodeVersion;
            FileUtil.XMLSave(UserConfigInstance.Instance.Config, UserConfigInstance.Instance.ConfigPath);
            MonitorDelegate("【保护线程】 更新成功");
            StartClient(true);
        }
    }
}
