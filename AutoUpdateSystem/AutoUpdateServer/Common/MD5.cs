using System.Security.Cryptography;
using System.IO;
using System.Text;
using System;

namespace AutoUpdateServer.Common
{
    public class MD5Helper
    {
        ///MD5加密
        public static string MD5Encode(string encryptString)
        {
            byte[] result = Encoding.Default.GetBytes(encryptString);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            string encryptResult = BitConverter.ToString(output).Replace("-", "");
            return encryptResult;
        }
    }
}