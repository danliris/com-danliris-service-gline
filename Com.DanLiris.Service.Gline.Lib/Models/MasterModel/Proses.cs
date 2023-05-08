using Com.Moonlay.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Com.DanLiris.Service.Gline.Lib.Models.MasterModel
{
    public class Proses : StandardEntity<Guid>
    {
        [Required]
        [MaxLength(255)]
        public string nama_proses { get; set; }
        public double cycle_time { get; set; }
    }
}
