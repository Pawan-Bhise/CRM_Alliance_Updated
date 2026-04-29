using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallCenter.Models
{
    public class Village
    {
        [Key]
        public string VillageCode { get; set; }
        public string VillageName { get; set; }

        [ForeignKey("VillageTract")]
        public string VillageTractCode { get; set; }
        public virtual VillageTract VillageTract { get; set; }
        public bool IsActive { get; set; }
    }

    //public class WardVillage
    //{
    //    [Key]
    //    public int Id { get; set; }

    //    public string SRName { get; set; }
    //    public string District { get; set; }
    //    public string Township { get; set; }
    //    public string TractTown { get; set; }
    //    public string WardEnglishName { get; set; }
    //    public string WardMMRName { get; set; }
    //    public string Ward_PCode { get; set; }
    //    public string VillageTractCode { get; set; }
    //}
}
