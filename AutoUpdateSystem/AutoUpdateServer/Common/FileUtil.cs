using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Serialization;

namespace AutoUpdateServer.Common
{
    public class FileUtil
    {
        public static void XMLSaveData<T>(T data, string filePath) where T : class
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            if (data != null)
            {
                XMLSave(data, filePath);
            }
        }

        public static T XMLLoadData<T>(string filePath) where T : class
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            try
            {
                var ser = new XmlSerializer(typeof(T));
                using (StreamReader sw = new StreamReader(filePath, Encoding.Unicode))
                {
                    var result = (T)ser.Deserialize(sw);
                    return result;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void XMLSave<T>(T data, string filePath) where T : class
        {
            var ser = new XmlSerializer(typeof(T));
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.Unicode))
            {
                ser.Serialize(sw, data);
            }
        }

        public static string TXTLoadData(string filePath)
        {
            using (StreamReader sw = new StreamReader(filePath, Encoding.Default))
            {
                var result = sw.ReadToEnd();
                return result;
            }
        }
        public static bool TXTSava(string filePath, string str)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.Write(str);
                return true;
            }
        }
    }
}