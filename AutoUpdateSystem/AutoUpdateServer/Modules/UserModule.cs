using AutoUpdateServer.Model;
using AutoUpdateServer.BLL;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoUpdateServer.Common;

namespace AutoUpdateServer.Modules
{
    public class UserModule:BaseModule
    {
        public UserModule() : base("User")
        {
            this.RequiresAuthentication();

            Get["UserManage"] = _ =>
            {
                return this.ValidPermission("UserManage") ? View["UserManage", UserBll.GetData()] : View["NoPermissions"];
            };
            Post["QueryUser"] = p =>
            {
                var name = Request.Form["name"];
                return View["UserManage", UserBll.UserLikeQuery(name)];
            };
            Post["CheckUserName/{Name}"] = p =>
            {
                return (UserBll.GetDataByName(p.Name) != null);
            };
            Get["UserAdd"] = _ =>
            {
                return this.ValidPermission("UserAdd") ? View["UserAdd"] : View["NoPermissions"];
            };
            Post["UserAdd"] = p =>
            {
                var userModel=new UserModel
                {
                    Name = Request.Form["UserName"],
                    PassWord = MD5Helper.MD5Encode(Request.Form["FirstPassword"]),
                    RoleName = Request.Form["RoleName"],
                };
                UserBll.Insert(userModel);
                return Response.AsRedirect("UserManage");
            };
            Get["UserEdit/{Name}"] = p =>
            {
                var model = UserBll.GetDataByName(p.Name);
                return this.ValidPermission("UserEdit") ? View["UserEdit", model] : View["NoPermissions"];
            };
            Post["UserEdit"] = _ =>
            {
                var userModel = new UserModel
                {
                    Name = Request.Form["UserName"],
                    RoleName = Request.Form["RoleName"],
                };
                UserBll.Update(userModel);
                return Response.AsRedirect("UserManage");
            };
            Post["UserDelete/{Name}"] = p =>
            {
                return UserBll.Delete(p.Name);
            };
            Get["UserPassWordReset/{Name}"] = p =>
            {
                return this.ValidPermission("UserPassWordReset") ? View["UserPassWordReset", p.Name] : View["NoPermissions"];
            };
            Post["UserPassWordReset"] = p =>
            {
                var userModel = new UserModel
                {
                    Name = Request.Form["UserName"],
                    PassWord = MD5Helper.MD5Encode(Request.Form["FirstPassword"]),
                    RoleName = Request.Form["RoleName"],
                };
                UserBll.ResetPassWord(userModel);
                return Response.AsRedirect("UserManage");
            };
        }
    }
}