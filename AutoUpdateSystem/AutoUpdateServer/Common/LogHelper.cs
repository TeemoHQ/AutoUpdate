using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Common
{
    public class LogHelper
    {
        public ILog Logger = LogManager.GetLogger("UpdateClient");
        private LogHelper()
        {

        }

        public static readonly LogHelper instance = new LogHelper();
    }
}