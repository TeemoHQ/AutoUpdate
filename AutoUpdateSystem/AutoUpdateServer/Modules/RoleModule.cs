using AutoUpdateServer.Model;
using AutoUpdateServer.BLL;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using AutoUpdateServer.Common;

namespace AutoUpdateServer.Modules
{
    public class RoleModule:BaseModule
    {
        public RoleModule() : base("Role")
        {
            this.RequiresAuthentication();

            Get["RoleManage"] = _ => ValidPermission("RoleManage") ? View["RoleManage", RoleBll.GetData()] : View["NoPermissions"];
            Post["QueryRole"] = p =>
            {
                string name = Request.Form["RoleName"];
                var roleList = RoleBll.RoleLikeQuery(name);
                return View["RoleManage", roleList];
            };
            Post["CheckRoleName/{Name}"] = p => RoleBll.GetDataByName(p.Name) != null;
            Get["RoleAdd"] = _ => ValidPermission("RoleAdd") ? View["RoleAdd"] : View["NoPermissions"];
            Post["RoleAdd"] = p =>
            {
                var roleModel = new RoleModel
                {
                    Name = Request.Form["RoleName"].ToString(),
                    CreateTime = DateTime.Now.ToString(),
                    CreateUer = Context.CurrentUser.UserName,
                    Permission = $"{Request.Form["Node"]},{Request.Form["group"]}",
                    Status = Request.Form["Status"].ToString(),
                    ManageHospital = string.IsNullOrEmpty(Request.Form["HospitalText"]) ? ConstFile.ALL : Request.Form["HospitalText"].ToString()
                };
                RoleBll.Insert(roleModel);
                return Response.AsRedirect("RoleManage");
            };
            Get["RoleEdit/{Name}"] = p =>
            {
                var model = RoleBll.GetDataByName(p.Name);
                return this.ValidPermission("RoleEdit") ? View["RoleEdit", model] : View["NoPermissions"];
            };
            Post["RoleEdit"] = _ =>
            {
                var roleModel = new RoleModel
                {
                    Name = Request.Form["Name"].ToString(),
                    CreateTime = DateTime.Now.ToString(),
                    CreateUer = Context.CurrentUser.UserName,
                    Permission = $"{Request.Form["Node"]},{Request.Form["group"]}",
                    Status = Request.Form["Status"].ToString(),
                    ManageHospital = string.IsNullOrEmpty(Request.Form["HospitalText"]) ? ConstFile.ALL : Request.Form["HospitalText"].ToString()
                };
                RoleBll.Update(roleModel);
                return Response.AsRedirect("RoleManage");
            };
            Post["RoleDelete/{Name}"] = p =>
            {
                return (RoleBll.Delete(p.Name));
            };
            Get["GetRolesToJson"] = _ =>
            {
                return Response.AsJson(RoleBll.GetBuiCheckBoxJson());
            };
        }
    }
}