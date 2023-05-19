using System;

namespace Com.DanLiris.Service.Gline.Lib.ViewModels.ReportViewModel
{
    public class RoOperatorHourlyWebReportViewModel
    {
        public string npk { get; set; }
        public Guid id_setting_ro { get; set; }
        public Guid id_proses { get; set; }
        public DateTime tanggal { get; set; }
        public string nama { get; set; }
        public string nama_proses { get; set; }
        public string range_waktu { get; set; }
        public int total_pass { get; set; }
        public int total_waktu_rework { get; set; }
        public int total_rework { get; set; }
    }
}
