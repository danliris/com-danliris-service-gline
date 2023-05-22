using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.ViewModels.ReportViewModel
{
    public class RoOperatorHourlyExcelReportViewModel 
    {
        public List<RoOperatorHourlyExcelViewModel> data { get; set; }
        public List<RoOperatorHourlyReworkExcelViewModel> dataRework { get; set; }
    }

    public class RoOperatorHourlyDataReportViewModel
    {   
        public List<RoOperatorHourlyReportViewModel> data { get; set; }
        public List<RoOperatorHourlyReworkReportViewModel> dataRework { get; set; }
    }

    public class RoOperatorHourlyPerDayReportViewModel
    {
        public DateTimeOffset tgl { get; set; }
        public int qty { get; set; }
    }

    public class RoOperatorHourlyPerDayPerHourReportViewModel
    {
        public string jam { get; set; }
        public int qty { get; set; }
    }

    public class RoOperatorHourlyPerDayPerHourPerRoReportViewModel
    {
        public string ro { get; set; }
        public int qty { get; set; }
    }

    public class RoOperatorHourlyReportViewModel
    {
        public TimeSpan waktu { get; set; }
        public string proses { get; set; } 
        public int pass { get; set; }
    }

    public class RoOperatorHourlyReworkReportViewModel
    {
        public DateTimeOffset tgl { get; set; }
        public string waktu { get; set; }
        public string proses { get; set; }
        public int rework { get; set;  }
    }

    public class RoOperatorHourlyExcelViewModel
    {
        public string nama { get; set; }
        public string ro { get; set; }
        public string proses { get; set; }
        public string shift { get; set; }
        public DateTime tgl { get; set; }
        public TimeSpan jam { get; set; }
        public int pass { get; set; }
    }

    public class RoOperatorHourlyReworkExcelViewModel
    {
        public string nama { get; set; }
        public string ro { get; set; }
        public string proses { get; set; }
        public string shift { get; set; }
        public DateTime tgl { get; set; }
        public string jam { get; set; }
    }
}
