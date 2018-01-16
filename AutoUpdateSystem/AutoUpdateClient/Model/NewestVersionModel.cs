using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdateClient.Model
{
    public class NewestVersionModel
    {
        public string FilePath { get; set; }

        public string Version { get; set; }

        public string DynamicCodeVersion { get; set; }

        public string DynamicCode { get; set; }

        public string BlackList { get; set; }

        public string ExistSoIgnoreList { get; set; }
    }
}
