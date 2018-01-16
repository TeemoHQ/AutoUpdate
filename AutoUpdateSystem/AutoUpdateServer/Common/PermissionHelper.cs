using AutoUpdateServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Common
{
    public class PermissionHelper
    {
        public static bool Validation(string permissions, string action)
        {
            return string.IsNullOrEmpty(permissions) ? false : permissions.Contains(action);
        }
    }
}