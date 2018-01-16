using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdateClient.Common
{
    public class ConstFile
    {
        public const string CLIENTEXENAME = "Terminal";
        public const string TEMP = "龖";
        public static string UserDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData");
        public static string PackagePath = Path.Combine(UserDataPath, "Package.7z");
        public static string NewAutoUpdateClientPath = Path.Combine(UserDataPath, "AutoUpdater.rar");
        public static string DefaultClientDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CLIENTEXENAME);
        public static string NewAutoupdateClientDirectory = Path.Combine(UserDataPath, "AutoUpdater");
        public static string TempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TEMP);
    }
}
