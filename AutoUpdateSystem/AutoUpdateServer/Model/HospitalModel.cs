using System.ComponentModel.DataAnnotations;

namespace AutoUpdateServer.Model
{
    public class HospitalModel : ModelBase
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string NewestVersion { get; set; }
    }
}