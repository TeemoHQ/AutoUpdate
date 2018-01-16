using AutoUpdateServer.Common;
using AutoUpdateServer.Model;
using AutoUpdateServer.Model.Request;
using AutoUpdateServer.Reponse.Model;
using Nancy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace AutoUpdateServer.BLL
{
    public class UpLoadFileBll
    {
        private List<string> _createFileOrDiretoryList = new List<string>();
        public ResponseModel BatchFile(string user, UpLoadFileRequestModel upLoadFileRequestModel)
        {
            int currentHosipitalId = upLoadFileRequestModel.HospitalId;
            try
            {
                //1.检查文件是否存在
                if (!File.Exists(upLoadFileRequestModel.FilePath))
                {
                    return ResponseModel.FailModel("文件不存在");
                }
                _createFileOrDiretoryList.Add(Path.GetDirectoryName(upLoadFileRequestModel.FilePath));
                FileControlCenter.Instance.RuningHospitalIDs.Add(currentHosipitalId);
                //2.设置新的文件和获取旧的文件的地址
                var tempDirectoryPath = Path.GetDirectoryName(upLoadFileRequestModel.FilePath);
                var newestFileDirectoryPath = Path.Combine(tempDirectoryPath, currentHosipitalId.ToString());
                var oldFilesDirectoryPath = Path.Combine(ConstFile.WorkPath, currentHosipitalId.ToString());
                //3.解压更新包到临时目录。
                if (ZipHelper.UnZip(upLoadFileRequestModel.FilePath, newestFileDirectoryPath))
                {
                    _createFileOrDiretoryList.Add(newestFileDirectoryPath);
                    //4.对比
                    CompareAction(oldFilesDirectoryPath, newestFileDirectoryPath, user, upLoadFileRequestModel);
                    FileControlCenter.Instance.RuningHospitalIDs.Remove(currentHosipitalId);
                    return ResponseModel.SuccessModel();
                }
                return ResponseModel.FailModel("解压失败");
            }
            catch (Exception ex)
            {
                FileControlCenter.Instance.RuningHospitalIDs.Remove(currentHosipitalId);
                return ResponseModel.FailModel($"上传失败：服务端异常:{ex}");
            }
            finally
            {
                //删除临时文件和目录
                DeleteTempFileOrDirectory();
            }
        }
        private static void CompareAction(string oldFilesDirectoryPath, string newestFileDirectoryPath, string user, UpLoadFileRequestModel upLoadFileRequestModel)
        {
            //1.获取当前医院最新版本设置版本号
            //2.对比,管理文件
            //3.修改本地配置
            var hospitalID = upLoadFileRequestModel.HospitalId;
            var lastVersionModel = new VersionModel();
            var number = ConstFile.BASEVERSION;
            var newestAllDLLVersionDictionary = new Dictionary<string, string>();
            var newestFileDirectoryInfo = new DirectoryInfo(newestFileDirectoryPath);
            GetFileNameDictionary(newestFileDirectoryInfo, newestAllDLLVersionDictionary, ConstFile.BASEVERSION, hospitalID.ToString());
            var newesterVsionModel = new VersionModel
            {
                //Id = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                HospitalId = hospitalID,
                UpLoadTime = DateTime.Now,
                User = user,
                BlackList = upLoadFileRequestModel.BlackList,
                ExistSoIgnoreList = upLoadFileRequestModel.ExistSoIgnoreList,
                DynamicCode = upLoadFileRequestModel.DynamicCode,
                DynamicCodeVersion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Description = upLoadFileRequestModel.Description
            };
            var versionModels = VersionBll.GetModelsByHospitalId(hospitalID);
            //对比逻辑：
            //1.第一次上传的时候或者当前没有出新文件的时候。和模板文件匹配.
            //2.第二次开始：
            //  A【修改】.上传文件和本地文件都存在，对比不同，则把最新的文件复制到work目录和仓库目录，并且设置上传文件version为最新version（存入数据库时）
            //  B【不变】.上传文件和本地文件都存在，对比相同，不复制文件，设置上传文件version为老版本文件的version（存入数据库时）
            //  C【新增】.上传文件存在，本地文件不存在，则把最新的文件复制到work目录和仓库目录，并且设置上传文件version为最新version（存入数据库时）
            //  D【删除】.上传文件不存在，本地文件存在。暂时不操作。
            var oldAllDLLVersionDictionary = new Dictionary<string, string>();
            var workPath = Path.Combine(ConstFile.WorkPath, hospitalID.ToString());

            if (versionModels.Count != 0)
            {
                lastVersionModel = versionModels.FirstOrDefault(p => p.Id == versionModels.Max(t => t.Id));
                number = AddVersion(lastVersionModel.Number);
                oldAllDLLVersionDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(lastVersionModel.AllDllVersion);
            }
            else
            {
                var baseModel = VersionBll.GetModelById(ConstFile.BASEMODELID);
                oldAllDLLVersionDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(baseModel.AllDllVersion);
                number = AddVersion(baseModel.Number);
            }


            var tempdllVersionDictionary = new Dictionary<string, string>();
            foreach (var item in newestAllDLLVersionDictionary)
            {
                var newestFilePath = Path.Combine(newestFileDirectoryPath, item.Key);
                var newestFileInfo = new FileInfo(newestFilePath);
                if (oldAllDLLVersionDictionary.Keys.Contains(item.Key) && oldAllDLLVersionDictionary[item.Key] == ConstFile.BASEVERSION)
                {
                    oldFilesDirectoryPath = ConstFile.BaseModelFilePath;
                }
                var localFilePath = Path.Combine(oldFilesDirectoryPath, item.Key);
                if (File.Exists(localFilePath))
                {
                    var localFileInfo = new FileInfo(localFilePath);
                    if (!IsTheSame(newestFileInfo, localFileInfo))
                    {
                        FileCopy(newestFileInfo, hospitalID.ToString(), number, workPath, item.Key);
                        tempdllVersionDictionary.Add(item.Key, number);
                    }
                    else
                    {
                        tempdllVersionDictionary.Add(item.Key, oldAllDLLVersionDictionary[item.Key]);
                    }
                    oldAllDLLVersionDictionary.Remove(item.Key);
                }
                else
                {
                    FileCopy(newestFileInfo, hospitalID.ToString(), number, workPath, item.Key);
                    tempdllVersionDictionary.Add(item.Key, number);
                }
            }
            // 向前兼容.保护配置
            //（删除功能暂时删除）
            //var deleteKey = new List<string>();
            //foreach (var item in oldAllDLLVersionDictionary)
            //{
            //    if (item.Value != ConstFile.BASEVERSION)
            //    {
            //        deleteKey.Add(item.Key);
            //    }
            //}
            //deleteKey.ForEach((p) =>
            //{
            //    oldAllDLLVersionDictionary.Remove(p);

            //    //var deleteFilePath = Path.Combine(workPath, p);
            //    //MaintainWorkDic(deleteFilePath);
            //});

            foreach (var key in tempdllVersionDictionary.Keys)
            {
                if (oldAllDLLVersionDictionary.Keys.Contains(key))
                {
                    oldAllDLLVersionDictionary[key] = tempdllVersionDictionary[key];
                    continue;
                }
                oldAllDLLVersionDictionary.Add(key, tempdllVersionDictionary[key]);
            }

            newesterVsionModel.Number = number;
            newesterVsionModel.AllDllVersion = JsonConvert.SerializeObject(oldAllDLLVersionDictionary);

            VersionBll.Insert(newesterVsionModel);
            VersionBll.UpdateHospitalNewestNumber(number, hospitalID);
            if (MemoryCenter.Instance.NewestHospitalVersionDic.ContainsKey(hospitalID))
            {
                MemoryCenter.Instance.NewestHospitalVersionDic[hospitalID] = number;
            }
            else
            {
                MemoryCenter.Instance.NewestHospitalVersionDic.Add(hospitalID, number);
            }
        }
        internal ResponseModel BatchBaseModelFile(IEnumerable<HttpFile> files, string userName)
        {
            var responseModel = new ResponseModel();
            foreach (var file in files)
            {
                try
                {
                    var tempFileName = DateTime.Now.ToString($"yyyyMMddHHmmssffff{Path.GetExtension(file.Name)}");
                    var zipPath = Path.Combine(ConstFile.TempPath, tempFileName);
                    SaveFile(file, zipPath);
                    #region 对基文件不进行删除操作
                    //if (Directory.Exists(ConstFile.BaseModelFilePath))
                    //{
                    //    Directory.Delete(ConstFile.BaseModelFilePath, true);
                    //}
                    #endregion
                    if (!ZipHelper.UnZip(zipPath, ConstFile.BaseModelFilePath))
                    {
                        responseModel.Success = false;
                        responseModel.Msg = string.Format("上传失败：解压失败");
                        break;
                    }
                    var newestAlldllVersionDictionary = new Dictionary<string, string>();
                    var newestFileDirectoryInfo = new DirectoryInfo(ConstFile.BaseModelFilePath);
                    GetFileNameDictionary(newestFileDirectoryInfo, newestAlldllVersionDictionary, ConstFile.BASEVERSION, ConstFile.BASEMODELID.ToString());
                    var baseVersion = new VersionModel
                    {
                        AllDllVersion = JsonConvert.SerializeObject(newestAlldllVersionDictionary),
                        User = userName,
                        UpLoadTime = DateTime.Now,
                        HospitalId = -1,
                        Number = ConstFile.BASEVERSION,
                    };
                    var model = VersionBll.GetModelById(ConstFile.BASEMODELID);
                    if (model == null)
                    {
                        VersionBll.Insert(baseVersion);
                    }
                    else
                    {
                        VersionBll.UpdateBaseModel(baseVersion);
                    }
                    responseModel.Success = true;
                }
                catch (Exception ex)
                {
                    responseModel.Success = false;
                    responseModel.Msg = $"上传失败：{ex.Message}";
                    break;
                }

            }

            DeleteTempFileOrDirectory();
            return responseModel;

        }
        private static string AddVersion(string newestVersion)
        {
            //1.version 命名规则(2.0.0)2不变，递增
            var arr = newestVersion.Split('.');
            var second = int.Parse(arr[1]);
            var third = int.Parse(arr[2]);
            return third < 9 ? $"2.{second}.{++third}" : $"2.{++second}.0";
        }
        private static bool IsTheSame(FileInfo f1, FileInfo f2)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash1, hash2;
                using (var stream = f1.OpenRead())
                    hash1 = md5.ComputeHash(stream);
                using (var stream = f2.OpenRead())
                    hash2 = md5.ComputeHash(stream);
                return hash1.SequenceEqual(hash2);
            }
        }
        private static void FileCopy(FileInfo newestFileInfo, string hospitalId, string version, string workPath, string path)
        {
            //复制成新的更新包文件(生成相对的文件目录，因为可能出现不同文件夹同名文件)，复制到WORK目录和WareHousePath目录
            var newestPackageFileName =
                $"{Path.GetFileNameWithoutExtension(newestFileInfo.Name)}-{version}{Path.GetExtension(newestFileInfo.Name)}";
            var newestPackageFileInfo = new FileInfo(Path.Combine(ConstFile.WareHousePath, hospitalId, Path.GetDirectoryName(path), newestPackageFileName));
            var newestFileDir = newestPackageFileInfo.Directory;
            if (!newestFileDir.Exists)
                newestFileDir.Create();
            newestFileInfo.CopyTo(newestPackageFileInfo.FullName, true);

            var workFilePath = Path.Combine(workPath, path);
            var workFileInfo = new FileInfo(workFilePath);
            var workFileDir = workFileInfo.Directory;
            if (!workFileDir.Exists)
                workFileDir.Create();
            newestFileInfo.CopyTo(workFileInfo.FullName, true);
        }
        private ResponseModel SaveFile(HttpFile file, string zipPath)
        {
            try
            {
                var fileInfo = new FileInfo(zipPath);
                if (!Directory.Exists(fileInfo.DirectoryName))
                {
                    Directory.CreateDirectory(fileInfo.DirectoryName);
                }
                using (FileStream fileStream = new FileStream(zipPath, FileMode.Create))
                {
                    file.Value.CopyTo(fileStream);
                    _createFileOrDiretoryList.Add(zipPath);
                }
                return ResponseModel.SuccessModel();
            }
            catch (Exception e)
            {
                LogHelper.instance.Logger.Info($"保存文件{file.Name}失败 原因:{e}");
                return ResponseModel.FailModel($"保存文件{file.Name}失败 原因:{e}");
            }

        }
        private void DeleteTempFileOrDirectory()
        {
            try
            {
                foreach (var path in _createFileOrDiretoryList)
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    else if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                }
            }
            catch (Exception)
            {

            }

        }
        public ResponseModel CheckFile(HttpFile file, string manageHospital, int hospitalId)
        {
            if (FileControlCenter.Instance.IsMaintain)
            {
                return ResponseModel.FailModel("管理员正在维护版本，请稍后上传");
            }
            var model = VersionBll.GetModelById(ConstFile.BASEMODELID);
            if (model == null)
            {
                return ResponseModel.FailModel("服务端没有配置过模板文件，请联系管理员");
            }

            if (manageHospital != ConstFile.ALL && !manageHospital.Split(',').Contains(hospitalId.ToString()))
            {
                return ResponseModel.FailModel("警告：请勿上传不属于你管理医院的压缩包");
            }
            var hospitalModel = HospitalBll.GetDataById(hospitalId);
            if (hospitalModel == null)
            {
                return ResponseModel.FailModel($"请先在网站创建医院ID为{hospitalId}的记录，看规则");
            }
            if (FileControlCenter.Instance.RuningHospitalIDs.Contains(hospitalId))
            {
                return ResponseModel.FailModel("该医院当前其他人员在操作，请稍后");
            }
            var tempDirectoryPath = Path.Combine(ConstFile.TempPath, DateTime.Now.ToString("yyyyMMddHHmmssffff"));
            var zipPath = Path.Combine(tempDirectoryPath, Guid.NewGuid() + file.Name.Substring(file.Name.LastIndexOf(".")));
            var saveRes = SaveFile(file, zipPath);
            if (!saveRes.Success)
            {
                return saveRes;
            }
            return new UploadPackageResponseModel { Success = true, FilePath = zipPath };
        }
        private static void GetFileNameDictionary(DirectoryInfo info, Dictionary<string, string> fileNameDictionary, string version, string Id)
        {
            //Unix使用斜杆/ 作为路径分隔符，而web应用最新使用在Unix系统上面，所以目前所有的网络地址都采用 斜杆/ 作为分隔符。
            //Windows由于使用 斜杆/ 作为DOS命令提示符的参数标志了，为了不混淆，所以采用 反斜杠\ 作为路径分隔符。
            var tag = Environment.OSVersion.Platform == PlatformID.Win32NT ? $@"\{Id}\" : $@"/{Id}/";
            foreach (var item in info.GetFiles())
            {
                LogHelper.instance.Logger.Info($"{item.FullName},{tag},{item.FullName.Substring(item.FullName.LastIndexOf(tag) + tag.Length)}");
                //因为不同的目录可能含有相同名称的文件。所以取路径作为KEY
                fileNameDictionary.Add(item.FullName.Substring(item.FullName.LastIndexOf(tag) + tag.Length), version);
            }
            foreach (var item in info.GetDirectories())
            {
                GetFileNameDictionary(item, fileNameDictionary, version, Id);
            }
        }
    }
}