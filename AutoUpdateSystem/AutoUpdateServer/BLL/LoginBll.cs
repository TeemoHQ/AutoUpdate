using AutoUpdateServer.Common;
using AutoUpdateServer.Model;
using System.Collections.Generic;
using System.Linq;

namespace AutoUpdateServer.BLL
{
    public class LoginBll
    {
        public static UserModel Verify(List<UserModel> models, string name, string passWord)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(passWord))
            {
                return null;
            }
            var model = models.FirstOrDefault(t => t.Name == name);
            if (model != null)
            {
                var md5PassWord = MD5Helper.MD5Encode(passWord);
                if (md5PassWord != model.PassWord)
                {
                    model = null;
                }
            }
            return model;
        }
    }
}

