using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using AutoUpdateClient.Model;

namespace AutoUpdateClient.Common
{
    public class PipeHelper
    {
        private static string _serverKey = "I am the one true server!";

        public static string Communication(string writeString)
        {
            try
            {
                using (var pipeClient = new NamedPipeClientStream(".", "AutoUpdatePipe", PipeDirection.InOut,
                    PipeOptions.None, TokenImpersonationLevel.Impersonation))
                {
                    pipeClient.Connect(500);
                    if (pipeClient.IsConnected)
                    {
                        var ss = new StreamString(pipeClient);
                        if (ss.ReadString() == _serverKey)
                        {
                            ss.WriteString(writeString);
                            return ss.ReadString();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Info($"命名管道通信异常:{e.Message}");
            }
            return null;
        }
    }
}
