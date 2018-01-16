using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Common
{
    public class MemoryCenter
    {
        private MemoryCenter()
        {
                
        }
        public static  readonly  MemoryCenter Instance=new MemoryCenter();
        public Dictionary<int, string> NewestHospitalVersionDic=new Dictionary<int, string>();
    }
}