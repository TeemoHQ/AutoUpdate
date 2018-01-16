using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdateClient.Model
{
    public class UserConfig
    {
        public string ServerUrl { get; set; }

        public string HospitalID { get; set; }

        public string TerminalVersion { get; set; }

        public string AutoUpdaterVersion { get; set; }
        
        public string DynamicCodeVersion { get; set; }

        public string CheckClientAliveTime { get; set; }

        public string CheckUpdateTime { get; set; }

        //public string BatFileName { get; set; }

        public bool StartOnPowerOn { get; set; }

        public bool FirstInstall { get; set; }

        public List<string> KillProcesses { get; set; }

        public List<string> CloseMainWindowProcesses { get; set; }
    }
}
