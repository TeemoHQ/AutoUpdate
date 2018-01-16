using AutoUpdateServer.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Model
{
    public class VersionModel : ModelBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int HospitalId { get; set; }
        public string Number { get; set; }
        public DateTime UpLoadTime { get; set; }
        public string User { get; set; }
        public string Description { get; set; }
        public string AllDllVersion { get; set; }
        public string BlackList { get; set; }
        public string ExistSoIgnoreList { get; set; }
        public string DynamicCode { get; set; }
        public string DynamicCodeVersion { get; set; }
    }
}