using AutoUpdateServer.Common;
using AutoUpdateServer.Model;
using AutoUpdateServer.Reponse.Model;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Modules
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            this.RequiresAuthentication();

            Get["index"] = _ =>
            {
                return View["index", this.Context.CurrentUser.UserName];
            };

        }
    }
}