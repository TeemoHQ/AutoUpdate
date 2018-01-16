using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Model
{
    public class ServerConfig
    {
        public DateTime LastAutoudaterUpdateTime { get; set; }
        public string AutoupdaterVersion { get; set; }
    }
}