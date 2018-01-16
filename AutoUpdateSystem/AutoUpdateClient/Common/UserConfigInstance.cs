using AutoUpdateClient.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdateClient.Common
{
    public class UserConfigInstance
    {

        private UserConfigInstance()
        {
            Init();
        }

        public static readonly UserConfigInstance Instance = new UserConfigInstance();

        public UserConfig Config { get; set; }

        public string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData", "UserConfig.xml");

        private void Init()
        {
            Config = FileUtil.XMLLoadData<UserConfig>(ConfigPath);
            if (Config == null)
            {
                Close();
            }
            //var type = Config.GetType();
            //var prop = type.GetProperties();
            //foreach (var p in prop)
            //{
            //    if (string.IsNullOrEmpty(p.GetValue(Config, null).ToString()))
            //    {
            //        Close();
            //    }
            //}
        }

        private static void Close()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            LogHelper.Instance.Logger.Info(string.Format("【请先正确配置UserConfig.xml文件】", DateTime.Now));
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
