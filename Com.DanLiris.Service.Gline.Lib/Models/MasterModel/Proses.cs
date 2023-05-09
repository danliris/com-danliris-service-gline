using Com.DanLiris.Service.Gline.Lib.Models.TransaksiModel;
using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Com.DanLiris.Service.Gline.Lib.Models.MasterModel
{
    public class Proses : StandardEntity<Guid>
    {
        [Required]
        [MaxLength(255)]
        public string nama_proses { get; set; }
        public double cycle_time { get; set; }

        public virtual ICollection<TransaksiOperator> TransaksiOperator { get; set; }
    }
}
