using Com.DanLiris.Service.Gline.Lib.Models.MasterModel;
using Com.DanLiris.Service.Gline.Lib.Models.TransaksiModel;
using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.Models.SettingRoModel
{
    public class SettingRo : StandardEntity<Guid>
    {
        [Required]
        [MaxLength(50)]
        public string rono { get; set; }
        public int jam_target { get; set; }
        public double smv { get; set; }
        [Required]
        [MaxLength(1000)]
        public string artikel { get; set; }
        public DateTime setting_date { get; set; }
        public TimeSpan setting_time { get; set; }
        public Guid id_line { get; set; }
        [Required]
        public int nama_line { get; set; }
        [Required]
        [MaxLength(32)]
        public string nama_gedung { get; set; }
        [Required]
        [MaxLength(32)]
        public string kode_unit { get; set; }
        [Required]
        [MaxLength(50)]
        public string nama_unit { get; set; }
        public int quantity { get; set; }
    }
}
