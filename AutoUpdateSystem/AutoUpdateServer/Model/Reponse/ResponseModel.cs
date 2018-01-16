using AutoUpdateServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Reponse.Model
{
    public class ResponseModel
    {
        public ResponseModel()
        {
            
        }

        public ResponseModel(bool success,string msg)
        {
            Success = success;
            Msg = msg;
        }

        public bool Success { get; set; }
    
        public string Msg { get; set; }

        public static ResponseModel SuccessModel()
        {
            return new ResponseModel(true,null);
        }

        public static  ResponseModel FailModel(string msg)
        {
            return new ResponseModel(true, msg);
        }

    }

    public class RequestNewestPackageUrlResponseModel : ResponseModel
    {
        public NewestVersionModel Data { get; set; } = new NewestVersionModel();
    }

    public class UploadPackageResponseModel : ResponseModel
    {
        public string FilePath { get; set; }
    }
}