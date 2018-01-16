using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Model
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