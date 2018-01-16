using System;
using System.ComponentModel.DataAnnotations;

namespace AutoUpdateServer.Model
{
    public class RoleModel: ModelBase
    {
        [Key]
        public string Name { get; set; }
        public string Permission { get; set; }
        public string CreateUer { get; set; }
        public string CreateTime { get; set; }
        public string Status { get; set; }
        public string ManageHospital { get; set; }
    }
}