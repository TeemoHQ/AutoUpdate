using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoUpdateClient.Model;
using Newtonsoft.Json;

namespace AutoUpdateClient.Common
{
    public class HttpHelper
    {
        private static HttpClient _httpClient = new HttpClient();
        public HttpHelper()
        {
            _httpClient.Timeout = new TimeSpan(0, 5, 0);
        }

        public static T Comunication<T>(string url) where T : Response, new()
        {
            try
            {
               var task = _httpClient.GetAsync(url);
                task.Result.EnsureSuccessStatusCode();
                // var httpResponse = await client.GetAsync(url);framework 4.5后支持
                HttpResponseMessage httpResponse = task.Result;
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    LogHelper.Info($" 请求结束，网络异常状态码：{httpResponse.StatusCode},地址:{url}");
                    return new T
                    {
                        Success = false,
                        Msg = $" 请求结束，网络异常状态码：{httpResponse.StatusCode},地址:{url}"
                    };
                }
                return JsonConvert.DeserializeObject<T>(httpResponse.Content.ReadAsStringAsync().Result);
            }
            catch (Exception e)
            {
                LogHelper.Info($" 请求结束，地址:{url},异常{e.Message}");
                return new T
                {
                    Success = false,
                    Msg = $" 请求结束，地址:{url},异常{e.Message}"
                };
            }

        }
    }
}
