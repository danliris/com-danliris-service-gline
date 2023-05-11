using Com.DanLiris.Service.Gline.Lib.ViewModels.ReportViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.Interfaces
{
    public interface IReportFacade
    {
        Tuple<List<RoDoneReportViewModel>, int> GetRoDoneReport(string unit, int line, DateTimeOffset? dateFrom, DateTimeOffset? dateTo);
        MemoryStream GetRoDoneExcel(string unit, int line, DateTimeOffset dateFrom, DateTimeOffset dateTo);
        Tuple<List<RoHourlyReportViewModel>, int> GetRoHourlyReport(string unit, int line, DateTimeOffset? dateFrom, DateTimeOffset? dateTo);
        MemoryStream GetRoHourlyExcel(string unit, int line, DateTimeOffset dateFrom, DateTimeOffset dateTo);
    }
}
