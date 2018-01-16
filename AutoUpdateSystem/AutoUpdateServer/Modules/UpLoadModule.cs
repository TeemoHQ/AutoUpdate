using AutoUpdateServer.Common;
using AutoUpdateServer.Reponse.Model;
using AutoUpdateServer.BLL;
using Nancy;
using Nancy.Security;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoUpdateServer.Model;
using AutoUpdateServer.Model.Request;
using Nancy.ModelBinding;

namespace AutoUpdateServer.Modules
{
    public class UpLoadModule : BaseModule
    {
        public UpLoadModule() : base("UpLoad")
        {
            this.RequiresAuthentication();

            Get["UpLoadFile/{HospitalId}"] = f =>
            {
                var hospitalId =int.Parse(f.HospitalId);
                var  modle = new VersionModel();
                List<VersionModel> models = VersionBll.GetModelsByHospitalId(hospitalId);
                if (models != null && models.Count > 0)
                {
                    modle = models.FirstOrDefault(p => p.Id == models.Max(t => t.Id));
                }
                modle.HospitalId = hospitalId;
                return this.ValidPermission("UpLoadFile") ? View["UpLoadFile", modle] : View["NoPermissions"];
            };

            Post["API/UpLoadFile", true] = async (p, ct) =>
            {
                ResponseModel model = await Task.Run(() =>
                {
                    var user = (UserIdentity)this.Context.CurrentUser;
                    var upLoadFileViewModel = new UpLoadFileBll();
                    var bindModel = this.Bind<UpLoadFileRequestModel>();
                    return upLoadFileViewModel.BatchFile(user.UserName, bindModel);
                });
                return Response.AsJson(model);
            };

            Post["API/CheckFile/{HospitalId}", true] = async (p, ct) =>
            {
                ResponseModel model = await Task.Run(() =>
                {
                    var manageHospital = GetManageHospitalIDs();
                    var upLoadFileViewModel = new UpLoadFileBll();
                    return upLoadFileViewModel.CheckFile(Request.Files.FirstOrDefault(), manageHospital,int.Parse(p.HospitalId));
                });
                return Response.AsJson(model);
            };

            Get["UpLoadBaseModelFile"] = _ =>
            {
                if (!ValidPermission("UpLoadBaseModelFile"))
                {
                    return View["NoPermissions"];
                }
                if (VersionBll.GetModelById(ConstFile.BASEMODELID) == null)
                {
                    return View["UpLoadBaseModelFile"];
                }
                return "已经存在基础模板啦~!基础模板不允许替换啦";
            };

            Post["API/UpLoadBaseModelFile"] = p =>
            {
                var model = new ResponseModel();
                if (FileControlCenter.Instance.RuningHospitalIDs.Count > 0)
                {
                    model.Success = false;
                    model.Msg = "当前有客户在操作。";
                }
                else if (FileControlCenter.Instance.IsMaintain)
                {
                    model.Success = false;
                    model.Msg = "当前有管理员在操作。";
                }
                else
                {
                    FileControlCenter.Instance.IsMaintain = true;
                    var upLoadFileViewModel = new UpLoadFileBll();
                    model = upLoadFileViewModel.BatchBaseModelFile(Request.Files, this.Context.CurrentUser.UserName);
                    FileControlCenter.Instance.IsMaintain = false;
                }
                return Response.AsJson(model);
            };
        }
    }
}