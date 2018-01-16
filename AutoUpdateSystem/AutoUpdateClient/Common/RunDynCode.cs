using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoUpdateClient.Model;
using Microsoft.CSharp;

namespace AutoUpdateClient.Common
{
    public class RunDynCode
    {
        public static bool GrammarTest(string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return true;
                }

                var provider = new CSharpCodeProvider();
                var cParameters = new CompilerParameters();
                cParameters.ReferencedAssemblies.Add("mscorlib.dll");
                cParameters.ReferencedAssemblies.Add("System.dll");
                cParameters.ReferencedAssemblies.Add("System.Core.dll");
                cParameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
                cParameters.ReferencedAssemblies.Add("System.Xml.dll");
                cParameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
                cParameters.GenerateExecutable = false;
                cParameters.GenerateInMemory = true;
                var realCode = string.Format(@"
                                        using System;
                                        using System.IO;
                                        using System.Linq;
                                        using System.Windows.Forms;
                                        using System.Xml;
                                        using System.Xml.Serialization;
                                        using System.Collections.Generic;
                                        namespace DynCode{{
                                           public class DynRunClass{{
                                                public static void Run(){{
                                                    {0}
                                                }}
                                            }}
                                        }}", content);
                var result = provider.CompileAssemblyFromSource(cParameters, realCode);
                if (result.Errors.HasErrors)
                {
                    var b = new StringBuilder();
                    foreach (CompilerError err in result.Errors)
                    {
                        b.AppendLine(err.ErrorText);
                    }
                    return false;
                }
                // 通过反射，执行代码  
                var objAssembly = result.CompiledAssembly;
                var type = objAssembly.GetType("DynCode.DynRunClass");
                var method = type.GetMethod("Run", BindingFlags.Static | BindingFlags.Public);
                method?.Invoke(null, null);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
