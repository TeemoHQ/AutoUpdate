using AutoUpdateServer.Common;
using AutoUpdateServer.Model;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using AutoUpdateServer.DAL;

namespace AutoUpdateServer.BLL
{
    public class VersionBll
    {
        public static List<VersionModel> GetModelsByHospitalId(int hospitalId)
        {
            return DataAccessCenter.DbContext.Version.Where(p => p.HospitalId == hospitalId).ToList();
        }
        public static VersionModel GetModelById(int id)
        {
            return DataAccessCenter.DbContext.Version.FirstOrDefault(p => p.Id == id);
        }
        public static bool Insert(VersionModel model)
        {
            if (model == null) return false;
            DataAccessCenter.DbContext.Version.Add(model);
            return DataAccessCenter.DbContext.SaveChanges() != 0;
        }
        public static bool Update(VersionModel model)
        {
            var oldModel = DataAccessCenter.DbContext.Version.FirstOrDefault(p => p.Id == model.Id);
            if (oldModel == null) return false;
            oldModel.Description = model.Description;
            oldModel.DynamicCode = model.DynamicCode;
            oldModel.DynamicCodeVersion = model.DynamicCodeVersion;
            return DataAccessCenter.DbContext.SaveChanges() != 0;
        }
        public static bool UpdateHospitalNewestNumber(string newestVersion, int hospitalId)
        {
            var model = DataAccessCenter.DbContext.Hospital.FirstOrDefault(p => p.Id == hospitalId);
            if (model == null) return false;
            model.NewestVersion = newestVersion;
            return DataAccessCenter.DbContext.SaveChanges() != 0;
        }

        public static bool UpdateBaseModel(VersionModel model)
        {
            var oldModel = DataAccessCenter.DbContext.Version.FirstOrDefault(p => p.Id == model.Id);
            if (oldModel == null) return false;
            oldModel.HospitalId = model.HospitalId;
            oldModel.Number = model.Number;
            oldModel.UpLoadTime = model.UpLoadTime;
            oldModel.User = model.User;
            oldModel.Description = model.Description;
            oldModel.AllDllVersion = model.AllDllVersion;
            return DataAccessCenter.DbContext.SaveChanges() != 0;
        }

        #region 删除功能暂时禁用（原因：打包的时候，有人去删除文件。引起冲突）
        //public static string Delete(string versionId, int hospitalId)
        //{
        //    var errorMsg = string.Empty;
        //    try
        //    {
        //        var versionModels = GetModelsByHospitalId(hospitalId);
        //        var currentVersionModel = versionModels?.FirstOrDefault(p => p.Id ==int.Parse(versionId) );
        //        if (currentVersionModel != null)
        //        {
        //            var nextNumber = AddVersion(currentVersionModel.Number);
        //            var nextVersionModel = versionModels?.FirstOrDefault(p => p.Number == nextNumber && p.HospitalId == hospitalId);
        //            versionModels.Remove(currentVersionModel);
        //            var currentAllDllVersionDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(currentVersionModel.AllDllVersion);
        //            if (nextVersionModel != null)
        //            {
        //                //删除本次更新中文件,如果下一个版本有用到，就不删除.
        //                var nextAllDllVersionDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(nextVersionModel.AllDllVersion);
        //                foreach (var item in currentAllDllVersionDictionary)
        //                {
        //                    if (!nextAllDllVersionDictionary.Contains(item))
        //                    {
        //                        var destFileFileName =
        //                            $"{Path.GetFileNameWithoutExtension(item.Key)}-{item.Value}{Path.GetExtension(item.Key)}";
        //                        var destFilePath = Path.Combine(ConstFile.WareHousePath, hospitalId.ToString(), Path.GetDirectoryName(item.Key), destFileFileName); ;
        //                        if (File.Exists(destFilePath))
        //                        {
        //                            File.Delete(destFilePath);
        //                            DeleteDirectory(destFilePath);
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                string newestNumber = string.Empty;
        //                var lastVersionModel = versionModels.FirstOrDefault(p => p.Id == versionModels.Max(s => s.Id));
        //                if (lastVersionModel != null)
        //                {
        //                    var lastAllDllVersionDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(lastVersionModel.AllDllVersion);
        //                    //当前为最新版本，删除本次更新，回滚到上一个版本,修改当前医院的最新版字段
        //                    foreach (var item in currentAllDllVersionDictionary)
        //                    {
        //                        if (!lastAllDllVersionDictionary.Contains(item))
        //                        {
        //                            var deleteDestWareHouseFileName =
        //                                $"{Path.GetFileNameWithoutExtension(item.Key)}-{currentVersionModel.Number}{Path.GetExtension(item.Key)}";
        //                            var deleteDestWareHouseFilePath = Path.Combine(ConstFile.WareHousePath, hospitalId.ToString(), Path.GetDirectoryName(item.Key), deleteDestWareHouseFileName);
        //                            if (File.Exists(deleteDestWareHouseFilePath))
        //                            {
        //                                File.Delete(deleteDestWareHouseFilePath);
        //                                DeleteDirectory(deleteDestWareHouseFilePath);
        //                            }
        //                            var deleteDestWorkFilePath = Path.Combine(ConstFile.WorkPath, hospitalId.ToString(), item.Key);
        //                            if (File.Exists(deleteDestWorkFilePath))
        //                            {
        //                                File.Delete(deleteDestWorkFilePath);
        //                                DeleteDirectory(deleteDestWorkFilePath);
        //                            }
        //                            if (lastAllDllVersionDictionary.Keys.Contains(item.Key))
        //                            {
        //                                var number = lastAllDllVersionDictionary[item.Key];
        //                                var rollBackDestFilePath = Path.Combine(ConstFile.WorkPath, hospitalId.ToString(), item.Key);
        //                                var rollBackSourceFilePath = string.Empty;
        //                                if (number == ConstFile.BASEVERSION)
        //                                {
        //                                    rollBackSourceFilePath = Path.Combine(ConstFile.BaseModelFilePath, item.Key);
        //                                }
        //                                else
        //                                {
        //                                    var rollBackSourceFileName =
        //                                        $"{Path.GetFileNameWithoutExtension(item.Key)}-{number}{Path.GetExtension(item.Key)}";
        //                                    rollBackSourceFilePath = Path.Combine(ConstFile.WareHousePath, hospitalId.ToString(), Path.GetDirectoryName(item.Key), rollBackSourceFileName);
        //                                }
        //                                var rollBackSourceFileInfo = new FileInfo(rollBackSourceFilePath);
        //                                rollBackSourceFileInfo.CopyTo(rollBackDestFilePath, true);
        //                            }
        //                        }
        //                    }
        //                    newestNumber = lastVersionModel.Number;
        //                }
        //                else
        //                {
        //                    //全部删除
        //                    if (Directory.Exists(Path.Combine(ConstFile.WorkPath, hospitalId.ToString())))
        //                        Directory.Delete(Path.Combine(ConstFile.WorkPath, hospitalId.ToString()), true);
        //                    if (Directory.Exists(Path.Combine(ConstFile.WareHousePath, hospitalId.ToString())))
        //                        Directory.Delete(Path.Combine(ConstFile.WareHousePath, hospitalId.ToString()), true);
        //                }
        //                UpdateHospitalNewestNumber(newestNumber, currentVersionModel.HospitalId);
        //            }
        //            //删除数据库表单数据
        //            MYSQLHelper.DeleteEntity("Version", "ID=@ID", new object[] { currentVersionModel.Id });
        //            if (MemoryCenter.Instance.NewestHospitalVersionDic.ContainsKey(hospitalId))
        //            {
        //                MemoryCenter.Instance.NewestHospitalVersionDic.Remove(hospitalId);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        errorMsg = ex.Message;
        //    }
        //    return errorMsg;
        //}
        //private static void DeleteDirectory(string filePath)
        //{
        //    var destDirectoryInfo = new DirectoryInfo(Path.GetDirectoryName(filePath));
        //    if (destDirectoryInfo.GetFiles().Count() <= 0 && destDirectoryInfo.GetDirectories().Count() <= 0)
        //    {
        //        Directory.Delete(Path.GetDirectoryName(filePath));
        //    }
        //}
        //private static string AddVersion(string version)
        //{
        //    var arr = version.Split('.');
        //    var second = int.Parse(arr[1]);
        //    var third = int.Parse(arr[2]);
        //    return third < 9 ? $"2.{second}.{++third}" : $"2.{++second}.0";
        //}
        #endregion
    }
}