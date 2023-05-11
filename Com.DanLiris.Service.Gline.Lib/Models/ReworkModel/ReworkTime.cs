using Com.Moonlay.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Com.DanLiris.Service.Gline.Lib.Models.ReworkModel
{
    public class ReworkTime : StandardEntity<Guid>
    {
        [Required]
        [MaxLength(32)]
        public string npk { get; set; }
        [Required]
        [MaxLength(32)]
        public string nama_operator { get; set; }
        public TimeSpan jam_awal { get; set; }
        public TimeSpan jam_akhir { get; set; }
        public Guid id_rework { get; set; }
    }
}
