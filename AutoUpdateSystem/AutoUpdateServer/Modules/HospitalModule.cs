using AutoUpdateServer.Model;
using AutoUpdateServer.BLL;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using AutoUpdateServer.Common;
using AutoUpdateServer.Model.Client;

namespace AutoUpdateServer.Modules
{
    public class HospitalModule : BaseModule
    {
        public HospitalModule() : base("Hospital")
        {
            this.RequiresAuthentication();

            #region Hospital

            Get["HospitalManage"] = _ =>
            {
                string manageHospital = GetManageHospitalIDs();
                return this.ValidPermission("HospitalManage")
                    ? View["HospitalManage", HospitalBll.GetData(manageHospital)]
                    : View["NoPermissions"];
            };
            Post["QueryHospital"] = p =>
            {
                var arg = Request.Form["arg"];
                string manageHospital = GetManageHospitalIDs();
                return View["HospitalManage", HospitalBll.HospitalLikeQuery(arg, manageHospital)];
            };
            Post["checkHospitalID/{ID}"] = p => (HospitalBll.GetDataById(p.ID) != null);
            Get["HospitalAdd"] = _ => this.ValidPermission("HospitalAdd") ? View["HospitalAdd"] : View["NoPermissions"];
            Post["HospitalAdd/{Name}"] = p =>
            {
                var model = new HospitalModel
                {
                    //Id = p.ID,
                    Name = p.Name
                };
                return JsonConvert.SerializeObject(HospitalBll.Insert(model));
            };
            Get["HospitalEdit/{ID}"] =
                p =>
                    this.ValidPermission("HospitalEdit")
                        ? View["HospitalEdit", HospitalBll.GetDataById(p.ID)]
                        : View["NoPermissions"];
            Post["HospitalEdit/"] = _ =>
            {
                var model = this.Bind<HospitalModel>();
                HospitalBll.Update(model);
                return Response.AsRedirect("HospitalManage");
            };
            Post["HospitalDelete/{ID}"] = p => JsonConvert.SerializeObject(HospitalBll.Delete(p.ID));

            #endregion

            #region Version

            Get["VersionManage/{HospitalID}"] = p =>
            {
                if (!this.ValidPermission("VersionManage"))
                {
                    return View["NoPermissions"];
                }
                int id = int.Parse(p.HospitalID);
                var versionModels = VersionBll.GetModelsByHospitalId(id);
                var hospitalModel = HospitalBll.GetDataById(p.HospitalID);
                ViewBag["NewestVersion"] = hospitalModel.NewestVersion;
                ViewBag["ClientFileName"] = p.HospitalID + "_AutoUpdater.zip";
                versionModels.Reverse();
                return View["VersionManage", versionModels];
            };
            Get["VersionEdit/{ID}"] = p =>
            {
                if (!this.ValidPermission("VersionEdit"))
                {
                    return View["NoPermissions"];
                }
                var model = VersionBll.GetModelById(p.ID);
                return (model != null) ? View["VersionEdit", model] : View["VersionManage/" + model.HospitalID];
            };
            Post["VersionEdit"] = _ =>
            {
                var model = new VersionModel
                {
                    Id= Request.Form["ID"],
                    Description = Request.Form["Description"],
                    BlackList = Request.Form["BlackList"],
                    ExistSoIgnoreList = Request.Form["ExistSoIgnoreList"],
                    DynamicCode = Request.Form["DynamicCode"],
                    DynamicCodeVersion = Request.Form["DynamicCodeVersion"]
                };
                VersionBll.Update(model);
                string url = string.Format("VersionManage/{0}", Request.Form["HospitalID"]);
                return Response.AsRedirect(url);
            };
            //Post["VersionDelete/{VersionID}/{HospitalID}"] = p =>
            //{
            //    var errorMsg = VersionBll.Delete(p.VersionID, p.HospitalID);
            //    return (!string.IsNullOrEmpty(errorMsg)) ? errorMsg : null;
            //};
            Post["VersionSet/{VersionNumber}/{HospitalID}"] =p => 
            {
                MemoryCenter.Instance.NewestHospitalVersionDic.Clear();
                return VersionBll.UpdateHospitalNewestNumber(p.VersionNumber, p.HospitalID);
            }
            ;
            Get["DownLoadAutoUpdateClient/{ClientFileName}"] = p =>
            {
                var errorMsg = string.Empty;
                var hospitalId = p.ClientFileName.ToString().Split('_')[0];
                var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
                try
                {
                    if (FileControlCenter.Instance.IsCreatClient)
                    {
                        return "服务器正忙，请稍等";
                    }
                    FileControlCenter.Instance.IsCreatClient = true;
                    if (!Directory.Exists(ConstFile.AutoUpdateClient))
                    {
                        FileInfo basefile = new DirectoryInfo(ConstFile.DownloadFilePath).GetFiles().FirstOrDefault(t => Path.GetFileNameWithoutExtension(t.Name) == "AutoUpdater");
                        if (basefile == null)
                        {
                            FileControlCenter.Instance.IsCreatClient = false;
                            return "服务端未发布自动更新客户端";
                        }
                        if (basefile.Extension.ToUpper() != ".7Z" && !isWindows)
                        {
                            FileControlCenter.Instance.IsCreatClient = false;
                            return "linux服务端请联系管理员发布7z版本自动更新客户端";
                        }
                        var zipFilePath = Path.Combine(ConstFile.DownloadFilePath, basefile.Name);
                        if (!ZipHelper.UnZip(zipFilePath, ConstFile.AutoUpdateClient))
                        {
                            FileControlCenter.Instance.IsCreatClient = false;
                            return "服务端解压失败";
                        }
                    }
                    var userConfig = new UserConfig();
                    userConfig.AutoUpdaterVersion = "1.0.0";
                    userConfig.CheckClientAliveTime = "5000";
                    userConfig.CheckUpdateTime = "120000";
                    userConfig.FirstInstall = true;
                    userConfig.HospitalID = hospitalId;
                    userConfig.TerminalVersion = "2.0.0";
                    userConfig.ServerUrl = Request.Url.SiteBase;
                    userConfig.StartOnPowerOn = true;
                    userConfig.CloseMainWindowProcesses=new List<string>{ConstFile.CLIENTEXENAME };
                    userConfig.KillProcesses = new List<string> { "fileServer","CameraService" };
                    var configPath = Path.Combine(ConstFile.AutoUpdateClient,
                        "UserData", "UserConfig.xml");
                    FileUtil.XMLSaveData(userConfig, configPath);
                    var fileName = isWindows ? $"{hospitalId}_AutoUpdater.zip" : $"{hospitalId}_AutoUpdater.7z";
                    var packagePath = Path.Combine(ConstFile.DownloadFilePath, fileName);
                    if (File.Exists(packagePath))
                    {
                        File.Delete(packagePath);
                    }
                    if (ZipHelper.Zip(ConstFile.AutoUpdateClient, fileName))
                    {
                        FileControlCenter.Instance.IsCreatClient = false;
                        return Response.AsFile(packagePath);
                        //return Response.FromStream(new FileStream(packagePath, FileMode.Open), "application/octet-stream;charset=UTF-8");
                    }
                    else
                    {
                        errorMsg = "服务端异常：解压失败";
                    }
                }
                catch (Exception ex)
                {
                    errorMsg = ex.Message;
                }
                FileControlCenter.Instance.IsCreatClient = false;
                return errorMsg;
            };
            #endregion
        }
    }
}