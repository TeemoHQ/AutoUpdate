using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoUpdateClient.Common
{
    public static class SystemHelper
    {
        /// <summary>
        /// 桌面快捷方式
        /// </summary>
        /// <param name="linkName"></param>
        public static void CreateShortcut(string linkName)
        {
            var deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var lnkName = linkName + ".lnk";


            CreateShotcut(System.Reflection.Assembly.GetExecutingAssembly().Location, Path.Combine(deskDir, lnkName));
        }

        /// <summary>
        /// 开机启动
        /// </summary>
        /// <param name="linkName"></param>
        public static void StartOnPowerOn(string linkName)
        {
            var deskDir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            var lnkName = linkName + ".lnk";

            CreateShotcut(System.Reflection.Assembly.GetExecutingAssembly().Location, Path.Combine(deskDir, lnkName));

        }

        /// <summary>
        /// 创建快捷方式
        /// </summary>
        /// <param name="sourcefilePath">需要创建快捷方式的文件</param>
        /// <param name="destFilePath">创建的目标地址</param>
        private static void CreateShotcut(string sourcefilePath, string destFilePath)
        {
            var shell = new WshShell();
            var shortcut = (IWshShortcut)shell.CreateShortcut(destFilePath);
            if (System.IO.File.Exists(destFilePath))
            {
                if (shortcut.TargetPath == sourcefilePath)
                {
                    return;
                }
            }
            var startups = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            var files = Directory.GetFiles(startups);
            var tmpshell = new WshShell();
            foreach (var file in files)
            {
                try
                {
                    if (!file.EndsWith(".lnk"))
                    {
                        continue;
                    }
                    if (((IWshShortcut)tmpshell.CreateShortcut(file)).TargetPath == sourcefilePath)
                    {
                        System.IO.File.Delete(file);
                    }
                }
                catch (Exception)
                {

                }
            }

            shortcut.TargetPath = sourcefilePath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(shortcut.TargetPath);
            shortcut.Save();
        }

    }
}
