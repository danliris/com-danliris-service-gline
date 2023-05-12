using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.ViewModels
{
    public class RoOngoingQcViewModel
    {
        public string rono { get; set; }
        public int jam_target { get; set; }
        public double smv { get; set; }
        public string artikel { get; set; }
        public DateTime setting_date { get; set; }
        public TimeSpan setting_time { get; set; }
        public string nama_unit { get; set; }
        public int total_pass_per_hari { get; set; }
        public int total_reject_per_hari { get; set; }
    }
}
