using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using AutoUpdateServer.Model;

namespace AutoUpdateServer.DAL
{
    public class MyContext : DbContext
    {
        public MyContext() : base("name=conn")
        {
        }

        public DbSet<HospitalModel> Hospital { get; set; }
        public DbSet<RoleModel> Role { get; set; }
        public DbSet<VersionModel> Version { get; set; }
        public DbSet<UserModel> User { get; set; }
    }
}