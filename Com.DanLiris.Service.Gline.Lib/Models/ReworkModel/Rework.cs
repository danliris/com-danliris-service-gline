using Com.Moonlay.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Com.DanLiris.Service.Gline.Lib.Models.ReworkModel
{
    public class Rework : StandardEntity<Guid>
    {
        [Required]
        [MaxLength(32)]
        public string npk { get; set; }
        [Required]
        [MaxLength(32)]
        public string nama_operator { get; set; }
        public int qty_rework { get; set; }
        public Guid id_ro { get; set; }
        [Required]
        [MaxLength(50)]
        public string rono { get; set; }
        public Guid id_line { get; set; }
        public int nama_line { get; set; }
        public Guid id_proses { get; set; }
        [Required]
        [MaxLength(255)]
        public string nama_proses { get; set; }
    }
}
