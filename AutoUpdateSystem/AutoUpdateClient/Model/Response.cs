using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdateClient.Model
{
    public class Response
    {
        public bool Success { get; set; }

        public string Msg { get; set; }
    }

    public class VersionResponse: Response
    {
        public NewestVersionModel Data { get; set; }
    }
}
