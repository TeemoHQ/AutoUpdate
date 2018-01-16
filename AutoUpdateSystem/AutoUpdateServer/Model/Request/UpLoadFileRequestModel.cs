using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Model.Request
{
    public class UpLoadFileRequestModel
    {
        public string BlackList { get; set; }
        public string ExistSoIgnoreList { get; set; }
        public string DynamicCode { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public int HospitalId { get; set; }
    }
}