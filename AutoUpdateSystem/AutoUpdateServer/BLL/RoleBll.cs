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
    public class RoleBll
    {
        public static List<RoleModel> GetData()
        {
            return DataAccessCenter.DbContext.Role.ToList();
        }
        public static bool Insert(RoleModel model)
        {
            DataAccessCenter.DbContext.Role.Add(model);
            return DataAccessCenter.DbContext.SaveChanges() != 0;
        }
        public static List<RoleModel> RoleLikeQuery(string name)
        {
            return DataAccessCenter.DbContext.Role.Where(p => p.Name.Contains(name)).ToList();
        }
        public static RoleModel GetDataByName(string name)
        {
            return DataAccessCenter.DbContext.Role.FirstOrDefault(p => p.Name == name);
        }
        public static bool Update(RoleModel roleModel)
        {
            var model = DataAccessCenter.DbContext.Role.FirstOrDefault(p => p.Name == roleModel.Name);
            if (model == null)
            {
                return false;
            }
            model.CreateTime = roleModel.CreateTime;
            model.CreateUer = roleModel.CreateUer;
            model.ManageHospital = roleModel.ManageHospital;
            model.Permission = roleModel.Permission;
            model.Status = roleModel.Status;
            return DataAccessCenter.DbContext.SaveChanges() != 0;
        }
        public static bool Delete(string name)
        {
            var model = DataAccessCenter.DbContext.Role.FirstOrDefault(p => p.Name == name);
            if (model == null)
            {
                return false;
            }
            DataAccessCenter.DbContext.Role.Remove(model);
            return DataAccessCenter.DbContext.SaveChanges() != 0;
        }
        public static List<BuiCheckBox> GetBuiCheckBoxJson()
        {
            //前端BUI 选者框 json格式   [{ text: '选项1',value: 'a'},{ text: '选项2',value: 'b'},]
            List<RoleModel> roleModleList = GetData();
            var models = new List<BuiCheckBox>();
            roleModleList.ForEach((p) =>
            {
                if (p.Status == "1")
                {
                    models.Add(new BuiCheckBox { Text = p.Name, Value = p.Name });
                }
            });
            return models;
        }
    }

    public class BuiCheckBox
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
}