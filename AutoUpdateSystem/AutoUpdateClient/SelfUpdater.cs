using AutoUpdateClient.Common;
using AutoUpdateClient.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoUpdateClient
{
    public class SelfUpdater
    {
        public static Action<string> UpdateSelfDelegate;
        private static AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private static string _newestAutoupdaterVersion;
        public static void UpdateSelf()
        {
            StateCenter.Instance.HasSelfUpdated = false;
            UpdateSelfDelegate("【自我更新】 开始");
            var oldDircInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            foreach (var item in oldDircInfo.GetFiles())
            {
                if (item.Extension == ".old")
                {
                    File.Delete(item.FullName);
                }
            }
            try
            {
                var url =$"{UserConfigInstance.Instance.Config.ServerUrl}/api/RequestNewestAutoupdater/{UserConfigInstance.Instance.Config.AutoUpdaterVersion}";
                var res = HttpHelper.Comunication<VersionResponse>(url);
                if (res.Success)
                {
                    if (UserConfigInstance.Instance.Config.AutoUpdaterVersion == res.Data.Version)
                    {
                        UpdateSelfDelegate("【自我更新】 当前自动更新客户端是最新版本");
                        StateCenter.Instance.HasSelfUpdated = true;
                        return;
                    }
                    _newestAutoupdaterVersion = res.Data.Version;
                    DownLoad(res.Data.FilePath);
                    _autoResetEvent.WaitOne();
                }
                else
                {
                    UpdateSelfDelegate($"【自我更新】 服务端返回:{res.Msg}");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Info($"【自我更新】 结束客户端出错，原因：{ex.Message}");
                UpdateSelfDelegate($"【自我更新】 结束客户端出错原因：{ex.Message}");
            }
        }
        private static void DownLoad(string filePath)
        {
            try
            {
                if (Directory.Exists(ConstFile.NewAutoupdateClientDirectory))
                {
                    Directory.Delete(ConstFile.NewAutoupdateClientDirectory, true);
                }
                UpdateSelfDelegate("【自我更新】 下载开始时间");
                var url = new Uri($"{UserConfigInstance.Instance.Config.ServerUrl}/{filePath}");
                WebHelper.DownloadFileCompletedAction = DownloadFileCompleted;
                WebHelper.DownLoad(url, ConstFile.NewAutoUpdateClientPath);
            }
            catch (WebException e)
            {
                LogHelper.Info($"【自我更新】 下载失败原因：网络异常{e}");
                UpdateSelfDelegate($"【自我更新】 下载失败原因：网络异常{e}");
            }
            catch (Exception ex)
            {
                LogHelper.Info($"【自我更新】 下载失败原因：{ex}");
                UpdateSelfDelegate($"【自我更新】 下载失败原因：{ex}");
            }
        }
        private static void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.UserState.ToString() == "AutoUpdate")
            {
                UpdateSelfDelegate("【自我更新】 下载文件成功时间");
                if (ZipHelper.UnZipPath(ConstFile.NewAutoUpdateClientPath, ConstFile.NewAutoupdateClientDirectory))
                {
                    var oldDircInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                    foreach (var item in oldDircInfo.GetFiles())
                    {
                        File.Move(item.FullName, item.FullName + ".old");
                    }
                    var nestDircInfo = new DirectoryInfo(ConstFile.NewAutoupdateClientDirectory);
                    foreach (var item in nestDircInfo.GetFiles())
                    {
                        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(item.FullName));
                        File.Move(item.FullName, path);
                    }
                    UserConfigInstance.Instance.Config.AutoUpdaterVersion = _newestAutoupdaterVersion;
                    FileUtil.XMLSave(UserConfigInstance.Instance.Config, UserConfigInstance.Instance.ConfigPath);
                    Process.Start(new ProcessStartInfo
                    {
                        WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                        FileName = "AutoUpdateClient.exe"
                    });
                    Environment.Exit(0);
                }
                else
                {
                    UpdateSelfDelegate("【自我更新】 解压失败");
                    LogHelper.Info("【自我更新】 解压失败");
                }
                _autoResetEvent.Set();
            }
        }
    }
}
