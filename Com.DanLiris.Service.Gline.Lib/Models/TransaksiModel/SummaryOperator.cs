using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.Models.TransaksiModel
{
    public class SummaryOperator : StandardEntity<Guid>
    {
        [Required]
        [MaxLength(32)]
        public string npk { get; set; }
        [Required]
        [MaxLength(32)]
        public string nama { get; set; }
        public int jml_pass_per_ro { get; set; }
        public int total_rework { get; set; }
        public TimeSpan total_waktu_pengerjaan { get; set; }
        public Guid id_ro { get; set; }
        [Required]
        [MaxLength(50)]
        public string rono { get; set; }
        public DateTime setting_date { get; set; }
        public Guid id_proses { get; set; }
        [Required]
        [MaxLength(255)]
        public string nama_proses { get; set; }


    }
}
