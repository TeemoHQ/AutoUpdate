using System;
using System.ComponentModel.DataAnnotations;

namespace AutoUpdateServer.Model
{
    [Serializable]
    public class UserModel : ModelBase
    {
        [Key]
        public string Name { get; set; }

        public string PassWord { get; set; }

        public string RoleName { get; set; }
    }
}