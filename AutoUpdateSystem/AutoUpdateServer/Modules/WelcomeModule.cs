using AutoUpdateServer.Model;
using AutoUpdateServer.Reponse.Model;
using AutoUpdateServer.BLL;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.ModelBinding;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutoUpdateServer.Moudles
{
    public class WelcomeModule : NancyModule
    {
        public WelcomeModule()
        {
            #region LoginRemote
            Get["/"] = _ =>
            {
                return View["Login"];
            };
            Get["Login"] = _ =>
            {
                //生成CSRF token.
                this.CreateNewCsrfToken();
                return View["Login"];
            };
            Post["Login"] = p =>
            {
                //CSRF token 检验
                this.ValidateCsrfToken();
                var loginModel = this.Bind<LoginModel>();
                var model = LoginBll.Verify(UserBll.GetData(), loginModel.Username, loginModel.Password);
                if (model != null && !string.IsNullOrEmpty(model.RoleName))
                {
                    var role = RoleBll.GetDataByName(model.RoleName);
                    if (role != null && role.Status != "0")
                    {
                        Guid guid = Guid.NewGuid();
                        //注意 nancy的session实际用的是cookie，所以最大4K字节
                        Context.Request.Session[guid.ToString()] = model;
                        return this.LoginAndRedirect(guid, fallbackRedirectUrl: "/index");
                    }
                }
                return View["Login", "false"];
            };
            Get["LoginOut"] = _ =>
            {
                Session.DeleteAll();
                return this.LogoutAndRedirect("~/");
            };
            #endregion

            #region ClientUpdate

            Get["api/RequestNewestPackageUrl/{HopitalID}/{OldNumber}"] = p =>
            {
                RequestNewestPackageUrlResponseModel res = ClientUpdateBll.RequestNewestPackageUrl(p.HopitalID, p.OldNumber);
                return Response.AsJson(res);
            };

            Get["api/RequestNewestAutoupdater/{OldVersion}"] = p =>
            {
                RequestNewestPackageUrlResponseModel res = ClientUpdateBll.RequestNewestAutoupdater(p.OldNumber);
                return Response.AsJson(res);
            };
            #endregion
        }


    }
}