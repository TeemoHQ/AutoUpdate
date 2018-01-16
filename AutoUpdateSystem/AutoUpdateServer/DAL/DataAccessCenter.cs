using System;
using System.Data.Entity;
using System.Linq;
using AutoUpdateServer.Model;

namespace AutoUpdateServer.DAL
{
    public class DataAccessCenter
    {
        public static void Load()
        {
            IDatabaseInitializer<MyContext> dbInitializer = null;
            if (DbContext.Database.Exists())
            {
                //如果model有改变
                dbInitializer = new DropCreateDatabaseIfModelChanges<MyContext>();
            }
            else
            {
                //总是先删除然后再创建
                dbInitializer = new DropCreateDatabaseAlways<MyContext>();
            }
            dbInitializer.InitializeDatabase(DbContext);
            if (!DbContext.User.Any())
            {
                DbContext.User.Add(new UserModel
                {
                    Name = "admin",
                    PassWord = "202CB962AC59075B964B07152D234B70",
                    RoleName = "Admin"
                });
            }
            if (!DbContext.Role.Any())
            {
                DbContext.Role.Add(new RoleModel
                {
                    Name = "Admin",
                    Permission = "RoleAdd,RoleEdit,RoleDelete,RoleManage,UserAdd,UserEdit,UserDelete,UserManage,UserPassWordReset,HospitalAdd,HospitalEdit,HospitalDelete,HospitalManage,VersionAdd,VersionEdit,VersionDelete,VersionManage,UpLoadBaseModelFile,UpLoadFile,RoleGroup,UserGroup,HospitalGroup,VersionGroup,UpLoadGroup",
                    CreateUer = "admin",
                    CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = "1",
                    ManageHospital = "ALL"
                });
            }
            DbContext.SaveChanges();
        }

        public static MyContext DbContext= new MyContext();
    }
}