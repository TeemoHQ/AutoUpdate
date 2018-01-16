using AutoUpdateServer.Common;
using AutoUpdateServer.BLL;
using Nancy;

namespace AutoUpdateServer.Modules
{
    public abstract class BaseModule : NancyModule
    {
        public BaseModule(string module) : base(module)
        {

        }
        protected bool ValidPermission(string action)
        {
            var roleName = ((UserIdentity)this.Context.CurrentUser).RoleName;
            if (string.IsNullOrEmpty(roleName))
            {
                return false;
            }
            var permisson = RoleBll.GetDataByName(roleName)?.Permission;
            if (string.IsNullOrEmpty(permisson))
            {
                return false;
            }
            return PermissionHelper.Validation(permisson, action);
        }
        protected string GetManageHospitalIDs()
        {
            var roleName = ((UserIdentity)this.Context.CurrentUser).RoleName;
            if (string.IsNullOrEmpty(roleName))
            {
                return null;
            }
            return RoleBll.GetDataByName(roleName)?.ManageHospital;
        }

    }
}