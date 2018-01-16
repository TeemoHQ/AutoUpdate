using AutoUpdateServer.Common;
using AutoUpdateServer.Model;
using AutoUpdateServer.Reponse.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AutoUpdateServer.DAL;

namespace AutoUpdateServer.BLL
{
    public class HospitalBll
    {
        public static List<HospitalModel> GetData(string manageHospital)
        {
            if (manageHospital.ToUpper() != ConstFile.ALL)
            {
                return DataAccessCenter.DbContext.Hospital.Where(p => manageHospital.Contains(p.Id.ToString())).ToList();
            }
            return DataAccessCenter.DbContext.Hospital.ToList();
        }
        public static List<HospitalModel> HospitalLikeQuery(string name, string manageHospital)
        {
            if (!string.Equals(ConstFile.ALL, manageHospital))
            {
                return DataAccessCenter.DbContext.Hospital.Where(p => p.Name.Contains(name) && manageHospital.Contains(p.Id.ToString())).ToList();
            }
            return DataAccessCenter.DbContext.Hospital.Where(p => p.Name.Contains(name)).ToList();
        }
        public static HospitalModel GetDataById(int id)
        {
            return DataAccessCenter.DbContext.Hospital.FirstOrDefault(p => p.Id == id);
        }
        public static ResponseModel Insert(HospitalModel model)
        {
            DataAccessCenter.DbContext.Hospital.Add(model);
            return new ResponseModel { Success = DataAccessCenter.DbContext.SaveChanges() != 0 };
        }
        public static ResponseModel Update(HospitalModel hospitalmodel)
        {
            var model = DataAccessCenter.DbContext.Hospital.FirstOrDefault(p => p.Id == hospitalmodel.Id);
            if (model == null) return new ResponseModel {Success = false};
            model.Name = hospitalmodel.Name;
            return new ResponseModel { Success = DataAccessCenter.DbContext.SaveChanges() != 0 };
        }
        public static ResponseModel Delete(int id)
        {
            var model = DataAccessCenter.DbContext.Hospital.FirstOrDefault(p => p.Id == id);
            if (model == null) return new ResponseModel {Success = false};
            DataAccessCenter.DbContext.Hospital.Remove(model);
            return new ResponseModel { Success = DataAccessCenter.DbContext.SaveChanges() != 0 };
        }
    }
}