using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.ComponentModel;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using System.Collections.Generic;
using AutoUpdateClient;
using AutoUpdateClient.Common;
using AutoUpdateClient.Model;

namespace AutoUpdateClient
{
    public class Modifier
    {
        public static VersionResponse VersionResponse;
        public static Action<string> ModifierDelegate;
        private static AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        public static void CheckUpdate(object state)
        {
            try
            {
                if (!StateCenter.Instance.HasRepalced|| !StateCenter.Instance.HasRunDynCoded)
                {
                    ModifierDelegate("【修改线程】 等待客户端空闲替换下载好的文件");
                    return;
                }
                SelfUpdater.UpdateSelf();
                ModifierDelegate("【修改线程】 开始请求");
                var url = $"{UserConfigInstance.Instance.Config.ServerUrl}/api/RequestNewestPackageUrl/{UserConfigInstance.Instance.Config.HospitalID}/{UserConfigInstance.Instance.Config.TerminalVersion}";
                VersionResponse = HttpHelper.Comunication<VersionResponse>(url);
                JudgeDynamicCodeVersion();
                if (VersionResponse.Success)
                {
                    DownLoad(VersionResponse.Data.FilePath);
                    _autoResetEvent.WaitOne();
                }
                else
                {
                    ModifierDelegate($"【修改线程】 请求结束：{VersionResponse.Msg}");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Logger.Info($"【修改线程】 请求结束：{ex.Message}");
                ModifierDelegate($"【修改线程】 请求结束：{ex.Message}");
            }
        }

        private static void DownLoad(string filePath)
        {
            try
            {
                var url = new Uri($"{UserConfigInstance.Instance.Config.ServerUrl}/{filePath}");
                if (File.Exists(ConstFile.PackagePath))
                {
                    File.Delete(ConstFile.PackagePath);
                }
                WebHelper.DownloadFileCompletedAction = DownloadFileCompleted;
                WebHelper.DownLoad(url,ConstFile.PackagePath);
            }
            catch (WebException e)
            {
                LogHelper.Info($"【修改线程】 下载失败原因：网络异常,返回:{e.Response},信息{e.Message},状态{e.Status}");
                ModifierDelegate("【修改线程】 下载失败原因：网络异常");
            }
            catch (Exception ex)
            {
                LogHelper.Info($"【修改线程】 下载失败原因：{ex}");
                ModifierDelegate("【修改线程】 下载失败");
            }
        }
        private static void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.UserState.ToString() == "AutoUpdate")
            {
                StateCenter.Instance.HasRepalced = false;
                ModifierDelegate("【修改线程】 下载文件成功");
                _autoResetEvent.Set();
            }
        }

        private static void JudgeDynamicCodeVersion()
        {
            if (!string.IsNullOrEmpty(VersionResponse?.Data?.DynamicCodeVersion))
            {
                if (string.IsNullOrEmpty(UserConfigInstance.Instance.Config.DynamicCodeVersion)
                    || DateTime.Parse(VersionResponse?.Data?.DynamicCodeVersion) !=
                    DateTime.Parse(UserConfigInstance.Instance.Config.DynamicCodeVersion))
                {
                    StateCenter.Instance.HasRunDynCoded = false;
                }
            }
        }
    }
}
