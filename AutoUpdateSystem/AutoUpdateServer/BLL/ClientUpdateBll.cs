using AutoUpdateServer.Common;
using AutoUpdateServer.Model;
using AutoUpdateServer.Reponse.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutoUpdateServer.BLL
{
    public class ClientUpdateBll
    {
        internal static RequestNewestPackageUrlResponseModel RequestNewestPackageUrl(dynamic hospitalId, dynamic oldNumber)
        {
            var res = new RequestNewestPackageUrlResponseModel();
            //黑名单,存在则忽略,动态代码
            InitFilterData(hospitalId, res);
            if (MemoryCenter.Instance.NewestHospitalVersionDic.ContainsKey(hospitalId) &&
                MemoryCenter.Instance.NewestHospitalVersionDic[hospitalId] == oldNumber)
            {
                res.Success = false;
                res.Msg = "已经是最新版本";
                return res;
            }
            var hosipitalModel = HospitalBll.GetDataById(hospitalId);
            if (hosipitalModel == null || string.IsNullOrEmpty(hosipitalModel?.NewestVersion))
            {
                res.Success = false;
                res.Msg = "无当前医院版本信息";
                return res;

            }

            if (hosipitalModel?.NewestVersion == oldNumber)
            {
                MemoryCenter.Instance.NewestHospitalVersionDic[hospitalId] = hosipitalModel?.NewestVersion;
                res.Success = false;
                res.Msg = "已经是最新版本";
                return res;
            }
            
            try
            {
                var newestVersion = (HospitalBll.GetDataById(hospitalId))?.NewestVersion;
                var packageName = string.Format("{0}_{1}", oldNumber, newestVersion);
                var packageDirectoryPath = Path.Combine(ConstFile.DownloadFilePath, hospitalId, packageName);
                string packagePath = string.Format("{0}.7z", packageDirectoryPath);
                #region 判断最近一小时有没有打过包
                if (File.Exists(packagePath))// && oldNumber != ConstFile.BASEVERSION
                {
                    var fileInfo = new FileInfo(packagePath);
                    if (ExecDateDiff(DateTime.Now, fileInfo.LastWriteTime) < 3600000)
                    {
                        res.Success = true;
                        res.Msg = "最近一小时已经打过包,传回历史包";
                        res.Data.Version = newestVersion;
                        res.Data.FilePath = packagePath.Replace(AppDomain.CurrentDomain.BaseDirectory, "");
                        return res;
                    }
                    File.Delete(packagePath);
                }
                #endregion
                if (FileControlCenter.Instance.IsZipping)
                {
                    res.Success = false;
                    res.Msg = "当前医院正在打包，请稍后";
                    return res;
                }
                FileControlCenter.Instance.IsZipping = true;
                if (!Directory.Exists(packageDirectoryPath))
                {
                    //如果是基版本的话，把整个文件压缩成压缩包传递过去
                    if (oldNumber == ConstFile.BASEVERSION)
                    {
                        CopyFolder(ConstFile.BaseModelFilePath, packageDirectoryPath);
                    }
                    else { Directory.CreateDirectory(packageDirectoryPath); }
                }
                List<VersionModel> versionList = VersionBll.GetModelsByHospitalId(hospitalId);
                var newestAllDllVersionDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(versionList.FirstOrDefault(p => p.Number == newestVersion)?.AllDllVersion);
                var oldAllDllVersionDic = (oldNumber == ConstFile.BASEVERSION) ?
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(VersionBll.GetModelById(ConstFile.BASEMODELID).AllDllVersion) :
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(versionList.FirstOrDefault(p => p.Number == oldNumber)?.AllDllVersion);
                foreach (var item in newestAllDllVersionDic)
                {
                    if (!oldAllDllVersionDic.Contains(item))
                    {
                        //如果版本是2.0.0就去模板文件中拿，其他区仓库拿，copy到包目录。
                        var number = newestAllDllVersionDic[item.Key];
                        string rollBackSourceFilePath;
                        if (number == ConstFile.BASEVERSION)
                        {
                            rollBackSourceFilePath = Path.Combine(ConstFile.BaseModelFilePath, item.Key);
                        }
                        else
                        {
                            var rollBackSourceFileName =
                                $"{Path.GetFileNameWithoutExtension(item.Key)}-{number}{Path.GetExtension(item.Key)}";
                            rollBackSourceFilePath = Path.Combine(ConstFile.WareHousePath, hospitalId.ToString(), Path.GetDirectoryName(item.Key), rollBackSourceFileName);
                        }
                        var rollBackSourceFileInfo = new FileInfo(rollBackSourceFilePath);
                        var destFilePath = Path.Combine(packageDirectoryPath, item.Key);
                        if (!Directory.Exists(Path.GetDirectoryName(destFilePath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));
                        }
                        rollBackSourceFileInfo.CopyTo(destFilePath, true);

                    }
                    if (oldAllDllVersionDic.Keys.Contains(item.Key))
                    {
                        oldAllDllVersionDic.Remove(item.Key);
                    }
                }
                #region 取消删除功能
                //需要删除的部分存到xml传送到客户端，让客户端去删除（）
                //if (oldAllDLLVersionDic.Count > 0)
                //{
                //    var keyList = new List<string>();
                //    var deleteFileConfigPath = Path.Combine(packageDirectoryPath, "DeleteFileConfig.xml");
                //    foreach (var key in oldAllDLLVersionDic.Keys)
                //    {
                //        keyList.Add(key);
                //    }
                //    FileUtil.XMLSaveData<List<string>>(keyList, deleteFileConfigPath);
                //}
                #endregion

                //Nancy的下载文件只能放在Content静态文件夹里面才可以访问(不包含目录的压缩包，方便客户端直接解压替换)
                if (ZipHelper.Zip(packageDirectoryPath, Path.GetFileName(packagePath)))
                {
                    res.Success = true;
                    res.Data.Version = newestVersion;
                    res.Data.FilePath = packagePath.Replace(AppDomain.CurrentDomain.BaseDirectory, "");
                    Directory.Delete(packageDirectoryPath, true);
                    MemoryCenter.Instance.NewestHospitalVersionDic[hospitalId] = hosipitalModel?.NewestVersion;
                }
                else
                {
                    res.Success = false;
                    res.Msg = "服务端出错压缩失败";
                }
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.Msg = "服务端出错：" + ex.Message;
            }
            FileControlCenter.Instance.IsZipping = false;
            return res;
        }
        private static void InitFilterData(dynamic hospitalId, RequestNewestPackageUrlResponseModel res)
        {
            List<VersionModel> versionModels = VersionBll.GetModelsByHospitalId(hospitalId);
            var versionModel = versionModels?.FirstOrDefault(p => p.Id == versionModels.Max(s=>s.Id));
            res.Data.DynamicCodeVersion = versionModel?.DynamicCodeVersion;
            res.Data.BlackList = versionModel?.BlackList;
            res.Data.ExistSoIgnoreList = versionModel?.ExistSoIgnoreList;
            res.Data.DynamicCode = versionModel?.DynamicCode;
            res.Data.Version = versionModel.Number;
        }
        private static double ExecDateDiff(DateTime dateBegin, DateTime dateEnd)
        {
            TimeSpan ts1 = new TimeSpan(dateBegin.Ticks);
            TimeSpan ts2 = new TimeSpan(dateEnd.Ticks);
            TimeSpan ts3 = ts1.Subtract(ts2).Duration();
            return ts3.TotalMilliseconds;
        }
        private static void CopyFolder(string sourcePath, string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            foreach (var item in Directory.GetFiles(sourcePath))
            {
                string destFile = Path.Combine(destPath, Path.GetFileName(item));
                File.Copy(item, destFile, true);
            }
            foreach (var item in Directory.GetDirectories(sourcePath))
            {
                string destDir = Path.Combine(destPath, Path.GetFileName(item));
                CopyFolder(item, destDir);
            }
        }
        internal static RequestNewestPackageUrlResponseModel RequestNewestAutoupdater(dynamic oldVersion)
        {
            try
            {
                var res = new RequestNewestPackageUrlResponseModel();
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config","ServerConfig.xml");
                var config = FileUtil.XMLLoadData<ServerConfig>(configPath);
                FileInfo basefile = new DirectoryInfo(ConstFile.DownloadFilePath).GetFiles().FirstOrDefault(t => Path.GetFileNameWithoutExtension(t.Name) == "AutoUpdater");
                if (basefile == null)
                {
                    res.Success = false;
                    res.Msg = "服务端未找到自动更新客户端压缩包";
                    return res;
                }
                var filePath = Path.Combine(ConstFile.CONTENTFILEDIRECTORY, "DownLoad", basefile.Name);
                if (config.LastAutoudaterUpdateTime != basefile.LastWriteTime)
                {
                    config.LastAutoudaterUpdateTime = basefile.LastWriteTime;
                    config.AutoupdaterVersion = AddVersion(config.AutoupdaterVersion);
                    FileUtil.XMLSaveData(config, configPath);
                }
                res.Success = true;
                res.Data = new NewestVersionModel
                {
                    FilePath = filePath,
                    Version = config.AutoupdaterVersion
                };
                return res;
            }
            catch (Exception ex)
            {
                return new RequestNewestPackageUrlResponseModel
                {
                    Success = false,
                    Msg = ex.Message
                };
            }

        }
        private static string AddVersion(string newestVersion)
        {
            //1.version 命名规则(2.0.0)2不变，递增
            var arr = newestVersion.Split('.');
            var second = int.Parse(arr[1]);
            var third = int.Parse(arr[2]);
            return third < 9 ? $"1.{second}.{++third}" : $"1.{++second}.0";
        }
    }
}