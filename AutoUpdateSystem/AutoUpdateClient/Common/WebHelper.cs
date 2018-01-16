using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdateClient.Common
{
    public class WebHelper
    {
        private  static WebClient _webClient=new WebClient();
        private static string _token = "AutoUpdate";
        public static Action<object,AsyncCompletedEventArgs> DownloadFileCompletedAction;

        public static void DownLoad(Uri url,string path)
        {
            _webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
            _webClient.DownloadFileCompleted += DownloadFileCompleted;
            _webClient.Encoding = Encoding.UTF8;
            _webClient.DownloadFileAsync(url, path, _token);
        }

        private static void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            DownloadFileCompletedAction( sender,e);
        }
    }
}
