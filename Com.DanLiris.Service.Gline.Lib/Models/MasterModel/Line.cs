using Com.DanLiris.Service.Gline.Lib.Models.SettingRoModel;
using Com.DanLiris.Service.Gline.Lib.Models.TransaksiModel;
using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.Models.MasterModel
{
    public class Line : StandardEntity<Guid>
    {
        [Required]
        public int nama_line { get; set; }
        [Required]
        [MaxLength(32)]
        public string nama_gedung { get; set; }
        [Required]
        [MaxLength(64)]
        public string kode_unit { get; set; }
        [Required]
        [MaxLength(512)]
        public string nama_unit { get; set; }

        public virtual ICollection<SettingRo> SettingRo { get; set; }
        public virtual ICollection<TransaksiOperator> TransaksiOperator { get; set; }

    }
    
}

