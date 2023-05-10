using Com.DanLiris.Service.Gline.Lib.Models.MasterModel;
using Com.DanLiris.Service.Gline.Lib.Models.SettingRoModel;
using Com.Moonlay.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.DanLiris.Service.Gline.Lib.Models.TransaksiModel
{
    public class TransaksiQc : StandardEntity<Guid>
    {
        [Required]
        [MaxLength(32)]
        public string npk { get; set; }
        [Required]
        [MaxLength(32)]
        public string nama { get; set; }
        [Required]
        [MaxLength(32)]
        public string employee_role { get; set; }
        public Guid id_line { get; set; }
        public int nama_line { get; set; }
        public Guid id_setting_ro { get; set; }
        [Required]
        [MaxLength(50)]
        public string rono { get; set; }
        public DateTime setting_date { get; set; }
        public TimeSpan setting_time { get; set; }
        public int quantity { get; set; }
        public Guid id_proses { get; set; }
        [Required]
        [MaxLength(255)]
        public string nama_proses { get; set; }
        public double cycle_time { get; set; }
        public bool? pass { get; set; }
        public TimeSpan? pass_time { get; set; }
        public bool? reject { get; set; }
        public TimeSpan? reject_time { get; set; }
        [MaxLength(32)]
        public string npk_reject { get; set; }
        [MaxLength(32)]
        public string nama_reject { get; set; }
        public Guid id_shift { get; set; }
        [Required]
        [MaxLength(32)]
        public string nama_shift { get; set; }
    }
}
