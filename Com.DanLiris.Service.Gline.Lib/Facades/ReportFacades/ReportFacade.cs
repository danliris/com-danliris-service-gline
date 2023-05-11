using Com.DanLiris.Service.Gline.Lib.Interfaces;
using Com.DanLiris.Service.Gline.Lib.Models.TransaksiModel;
using Com.DanLiris.Service.Gline.Lib.ViewModels.ReportViewModel;
using Com.DanLiris.Service.Gline.Lib.Helpers;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Com.DanLiris.Service.Gline.Lib.Facades.ReportFacades
{
    public class ReportFacade : IReportFacade
    {
        public readonly GlineDbContext dbContext;
        public readonly IServiceProvider serviceProvider;

        public ReportFacade(IServiceProvider serviceProvider, GlineDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
        }

        public IQueryable<RoDoneReportViewModel> GetQueryRoDone(string unit, int line, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            var query = (from a in dbContext.TransaksiQc
                         join b in dbContext.Line on a.id_line equals b.Id
                         where b.nama_unit == (!string.IsNullOrWhiteSpace(unit) ? unit : b.nama_unit)
                         && a.nama_line == (line <= 0 ? a.nama_line : line)
                         && a.CreatedUtc.Date >= dateFrom.Value.Date
                         && a.CreatedUtc.Date >= dateTo.Value.Date
                         && a.nama_proses == "QC ENDLINE"
                         && a.pass == true
                         && a.IsDeleted == false
                         && b.IsDeleted == false
                         select new TransaksiQc
                         {
                             rono = a.rono,
                             quantity = a.quantity
                         })
                         .GroupBy(x => new { x.rono }, (key, group) => 
                         new RoDoneReportViewModel 
                         { 
                             rono = key.rono,
                             qty_done = group.Count(x => x.rono == key.rono),
                             qty_order = group.FirstOrDefault().quantity,
                             qty_remaining = (group.FirstOrDefault().quantity - group.Count(x => x.rono == key.rono))
                         });

            return query.AsQueryable();
        }

        public IQueryable<RoHourlyReportViewModel> GetQueryRoHourly(string unit, int line, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            var queryTemp = (from a in dbContext.TransaksiQc
                             join b in dbContext.Line on a.id_line equals b.Id
                             where b.nama_unit == (!string.IsNullOrWhiteSpace(unit) ? unit : b.nama_unit)
                             && a.nama_line == (line <= 0 ? a.nama_line : line)
                             && a.CreatedUtc.Date >= dateFrom.Value.Date
                             && a.CreatedUtc.Date >= dateTo.Value.Date
                             && a.nama_proses == "QC ENDLINE"
                             && a.pass == true
                             && a.IsDeleted == false
                             && b.IsDeleted == false
                             select new TransaksiQc
                             {
                                 rono = a.rono,
                                 CreatedUtc = a.CreatedUtc.Date,
                                 pass_time = a.pass_time,
                                 pass = a.pass
                             });

            var querySum = new List<RoHourlyReportViewModel>();

            foreach(var i in General.Shift)
            {
                var queryPerShiftTemp =
                    queryTemp
                    .Where(x => x.pass_time >= i.from && x.pass_time <= i.to)
                    .GroupBy(x => new { x.rono, x.CreatedUtc}, (key, group) =>
                    new RoHourlyReportViewModel
                    {
                        tgl = key.CreatedUtc,
                        rono = key.rono,
                        jam = i.name,
                        qty = group.Count(x => x.CreatedUtc == key.CreatedUtc && x.rono == key.rono)
                    }).OrderBy(x=> x.tgl).ThenBy(x=>x.rono).ThenBy(x=> x.jam);

                querySum.AddRange(queryPerShiftTemp.ToList());
            }

            return querySum.AsQueryable();
        }

        public MemoryStream GetRoDoneExcel(string unit, int line, DateTimeOffset dateFrom, DateTimeOffset dateTo)
        {
            var data = GetQueryRoDone(unit, line, dateFrom, dateTo);

            DataTable result = new DataTable();

            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "RO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Qty Order", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Qty Done", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Qty Remaining", DataType = typeof(Double) });

            if (data.ToArray().Count() == 0)
                result.Rows.Add("", "", 0, 0, 0); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in data)
                {
                    index++;
                    result.Rows.Add(index, item.rono, item.qty_order, item.qty_done, item.qty_remaining);
                }
            }

            ExcelPackage package = new ExcelPackage();
            CultureInfo Id = new CultureInfo("id-ID");

            var sheet = package.Workbook.Worksheets.Add("Report");
            var col = (char)('A' + result.Columns.Count);
            string tglawal = dateFrom.ToOffset(new TimeSpan(7, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
            string tglakhir = dateTo.ToOffset(new TimeSpan(7, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));

            sheet.Cells[$"A1:{col}1"].Value = string.Format("LAPORAN RO DONE {0}", line == 0 ? "ALL LINE" : $"LINE {line}");
            sheet.Cells[$"A1:{col}1"].Merge = true;
            sheet.Cells[$"A1:{col}1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A1:{col}1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells[$"A1:{col}1"].Style.Font.Bold = true;
            sheet.Cells[$"A2:{col}2"].Value = string.Format("Periode {0} - {1}", tglawal, tglakhir);
            sheet.Cells[$"A2:{col}2"].Merge = true;
            sheet.Cells[$"A2:{col}2"].Style.Font.Bold = true;
            sheet.Cells[$"A2:{col}2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A2:{col}2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells[$"A3:{col}3"].Value = string.Format("KONFEKSI : {0}", unit);
            sheet.Cells[$"A3:{col}3"].Merge = true;
            sheet.Cells[$"A3:{col}3"].Style.Font.Bold = true;
            sheet.Cells[$"A3:{col}3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A3:{col}3"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            sheet.Cells["A5"].LoadFromDataTable(result, true, OfficeOpenXml.Table.TableStyles.Light16);

            MemoryStream stream = new MemoryStream();
            package.SaveAs(stream);
            return stream;
        }

        public Tuple<List<RoDoneReportViewModel>, int> GetRoDoneReport(string unit, int line, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            var data = GetQueryRoDone(unit, line, dateFrom, dateTo);

            return Tuple.Create(data.ToList(), data.Count());
        }

        public MemoryStream GetRoHourlyExcel(string unit, int line, DateTimeOffset dateFrom, DateTimeOffset dateTo)
        {
            var data = GetQueryRoHourly(unit, line, dateFrom, dateTo);

            DataTable result = new DataTable();

            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "RO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jam", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Qty", DataType = typeof(Double) });

            if (data.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", 0); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in data)
                {
                    index++;
                    result.Rows.Add(index, item.tgl.Date, item.rono, item.jam, item.qty);
                }
            }

            ExcelPackage package = new ExcelPackage();
            CultureInfo Id = new CultureInfo("id-ID");

            var sheet = package.Workbook.Worksheets.Add("Report");
            var col = (char)('A' + result.Columns.Count);
            string tglawal = dateFrom.ToOffset(new TimeSpan(7, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
            string tglakhir = dateTo.ToOffset(new TimeSpan(7, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));

            sheet.Cells[$"A1:{col}1"].Value = string.Format("LAPORAN PER JAM RO {0}", line == 0 ? "ALL LINE" : $"LINE {line}");
            sheet.Cells[$"A1:{col}1"].Merge = true;
            sheet.Cells[$"A1:{col}1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A1:{col}1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells[$"A1:{col}1"].Style.Font.Bold = true;
            sheet.Cells[$"A2:{col}2"].Value = string.Format("Periode {0} - {1}", tglawal, tglakhir);
            sheet.Cells[$"A2:{col}2"].Merge = true;
            sheet.Cells[$"A2:{col}2"].Style.Font.Bold = true;
            sheet.Cells[$"A2:{col}2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A2:{col}2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells[$"A3:{col}3"].Value = string.Format("KONFEKSI : {0}", unit);
            sheet.Cells[$"A3:{col}3"].Merge = true;
            sheet.Cells[$"A3:{col}3"].Style.Font.Bold = true;
            sheet.Cells[$"A3:{col}3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A3:{col}3"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            sheet.Cells["A5"].LoadFromDataTable(result, true, OfficeOpenXml.Table.TableStyles.Light16);

            MemoryStream stream = new MemoryStream();
            package.SaveAs(stream);
            return stream;
        }

        public Tuple<List<RoHourlyReportViewModel>, int> GetRoHourlyReport(string unit, int line, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            var data = GetQueryRoHourly(unit, line, dateFrom, dateTo);

            return Tuple.Create(data.ToList(), data.Count());
        }
    }
}
