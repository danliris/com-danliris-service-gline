using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.Models.TransaksiModel
{
    public class SummaryQc : StandardEntity<Guid>
    {
        [Required]
        [MaxLength(32)]
        public string npk { get; set; }
        [Required]
        [MaxLength(32)]
        public string nama { get; set; }
        public int total_pass { get; set; }
        public int total_reject { get; set; }
        [Required]
        [MaxLength(50)]
        public string rono { get; set; }
        public TimeSpan setting_time { get; set; }
        public DateTime setting_date { get; set; }
    }
}
