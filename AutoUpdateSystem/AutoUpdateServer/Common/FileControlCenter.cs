using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace AutoUpdateServer.Common
{
    public class FileControlCenter
    {
        public readonly static FileControlCenter Instance = new FileControlCenter();
        private FileControlCenter()
        { }

        public List<int> RuningHospitalIDs = new List<int>();

        public bool IsMaintain = false;

        public bool IsCreatClient = false;

        public bool IsZipping = false;
    }
}

