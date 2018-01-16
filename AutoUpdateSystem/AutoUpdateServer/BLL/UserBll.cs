using AutoUpdateServer.Common;
using AutoUpdateServer.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoUpdateServer.DAL;

namespace AutoUpdateServer.BLL
{
    public class UserBll
    {
        public static List<UserModel> GetData()
        {
            return DataAccessCenter.DbContext.User.ToList();
        }
        public static UserModel GetDataByName(string name)
        {
            return DataAccessCenter.DbContext.User.FirstOrDefault(p => p.Name == name);
        }
        public static List<UserModel> UserLikeQuery(string name)
        {
            return DataAccessCenter.DbContext.User.Where(p => p.Name.Contains(name)).ToList();
        }
        public static bool Insert(UserModel userModel)
        {
            DataAccessCenter.DbContext.User.Add(userModel);
            return DataAccessCenter.DbContext.SaveChanges() != 0;
        }
        public static bool Update(UserModel userModel)
        {
            var model = DataAccessCenter.DbContext.User.FirstOrDefault(p => p.Name == userModel.Name);
            if (model != null)
            {
                model.RoleName = userModel.RoleName;
                return DataAccessCenter.DbContext.SaveChanges() != 0;
            }
            return false;
        }
        public static bool Delete(string name)
        {
            var model = DataAccessCenter.DbContext.User.FirstOrDefault(p => p.Name == name);
            if (model == null)
            {
                return false;
            }
            DataAccessCenter.DbContext.User.Remove(model);
            return DataAccessCenter.DbContext.SaveChanges() != 0;
        }
        internal static bool ResetPassWord(UserModel userModel)
        {
            var model = DataAccessCenter.DbContext.User.FirstOrDefault(p => p.Name == userModel.Name);
            if (model != null)
            {
                model.PassWord = userModel.PassWord;
                return DataAccessCenter.DbContext.SaveChanges() != 0;
            }
            return false;
        }
    }
}