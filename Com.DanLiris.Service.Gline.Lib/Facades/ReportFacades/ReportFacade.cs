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

        #region query
        private IQueryable<RoDoneReportViewModel> GetQueryRoDone(string unit, int line, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
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

        private IQueryable<RoHourlyReportViewModel> GetQueryRoHourly(string unit, int line, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
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

        private IQueryable<RoDetailOptReportViewModel> GetQueryRoDetailOpt(string ro, string unit, int line, DateTimeOffset? date)
        {
            var queryTempOpt = (from a in dbContext.TransaksiOperator
                                join b in dbContext.Line
                                on a.id_line equals b.Id
                                where a.CreatedUtc.Date == date.Value.Date
                                && (string.IsNullOrWhiteSpace(ro) || a.rono.Contains(ro))
                                && (string.IsNullOrWhiteSpace(unit) || b.nama_unit == unit)
                                && (line != 0 || b.nama_line == line)
                                && a.IsDeleted == false
                                && b.IsDeleted == false
                                group new { tgl = a.CreatedUtc.Date, rono = a.rono, nama = a.nama, line = b.nama_line, unit = b.nama_unit } by new { a.CreatedUtc.Date, a.rono, a.nama, b.nama_line, b.nama_unit } into G
                                select new RoDetailOptReportViewModel 
                                { 
                                    tgl = G.Key.Date,
                                    nama = G.Key.nama,
                                    pass = G.Count(x=> x.rono == G.Key.rono),
                                    reject = 0,
                                    rono = G.Key.rono
                                }).ToList();

            var queryTempQc = (from a in dbContext.TransaksiQc
                               join b in dbContext.Line
                               on a.id_line equals b.Id
                               where a.CreatedUtc.Date == date.Value.Date
                               && a.reject == true
                               && (string.IsNullOrWhiteSpace(ro) || a.rono.Contains(ro))
                               && (string.IsNullOrWhiteSpace(unit) || b.nama_unit == unit)
                               && (line != 0 || b.nama_line == line)
                               && a.IsDeleted == false
                               && b.IsDeleted == false
                               group new { tgl = a.CreatedUtc.Date, rono = a.rono, nama = a.nama_reject, line = b.nama_line, unit = b.nama_unit } by new { a.CreatedUtc.Date, a.rono, a.nama_reject, b.nama_line, b.nama_unit } into G
                               select new RoDetailOptReportViewModel
                               {
                                   tgl = G.Key.Date,
                                   nama = G.Key.nama_reject,
                                   pass = 0,
                                   reject = G.Count(x => x.rono == G.Key.rono),
                                   rono = G.Key.rono
                               }).ToList();

            var queryTemp = queryTempOpt.Union(queryTempQc);

            var querySum = queryTemp
                           .GroupBy(x => new { x.tgl, x.rono, x.nama }, (key, group) =>
                           new RoDetailOptReportViewModel
                           {
                               tgl = key.tgl,
                               nama = key.nama,
                               rono = key.rono,
                               pass = group.Sum(x => x.pass),
                               reject = group.Sum(x => x.reject)
                           });

            return querySum.AsQueryable();
        }

        private IQueryable<RoOperatorHourlyWebReportViewModel> GetQueryRoOperatorHourlyWeb(string area, int line, string proses, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            var queryTempOpt = (from a in dbContext.TransaksiOperator
                                join b in dbContext.Line
                                on a.id_line equals b.Id
                                where a.CreatedUtc.Date >= dateFrom.Value.Date
                                && a.CreatedUtc.Date <= dateTo.Value.Date
                                && (string.IsNullOrWhiteSpace(area) || b.nama_unit == area)
                                && (line == 0 || b.nama_line == line)
                                && (string.IsNullOrWhiteSpace(proses) || a.nama_proses == proses)
                                && a.IsDeleted == false
                                && b.IsDeleted == false
                                select a)
                                .GroupBy(x => new { x.id_setting_ro, x.rono, x.id_line, x.nama_line, x.id_proses, x.nama_proses, x.npk, x.CreatedUtc.Date, x.pass_time.Hours, x.nama }, (key, group) =>
                                new RoOperatorHourlyWebReportViewModel
                                {
                                    npk = key.npk,
                                    id_setting_ro = key.id_setting_ro,
                                    id_proses = key.id_proses,
                                    tanggal = key.Date,
                                    nama = key.nama,
                                    nama_proses = key.nama_proses,
                                    range_waktu = FormatHour(key.Hours),
                                    total_pass = group.Count(x => x.pass)
                                });

            var queryTempRework = (from a in dbContext.Line
                                   join b in dbContext.Rework
                                   on a.Id equals b.id_line
                                   join c in dbContext.ReworkTime
                                   on b.Id equals c.id_rework
                                   where b.CreatedUtc.Date >= dateFrom.Value.Date
                                   && b.CreatedUtc.Date <= dateTo.Value.Date
                                   && (string.IsNullOrWhiteSpace(area) || a.nama_unit == area)
                                   && (line == 0 || b.nama_line == line)
                                   && a.IsDeleted == false
                                   && b.IsDeleted == false
                                   && c.IsDeleted == false
                                   select new
                                   {
                                       b.npk,
                                       b.CreatedUtc.Date,
                                       b.id_ro,
                                       c.jam_awal.Hours,
                                       b.id_proses,
                                       b.nama_proses,
                                       c.nama_operator,
                                       c.jam_awal,
                                       c.jam_akhir
                                   })
                                   .GroupBy(x => new { x.id_ro, x.id_proses, x.nama_proses, x.npk, x.Date, x.Hours, x.nama_operator, x.jam_awal, x.jam_akhir }, (key, group) =>
                                   new RoOperatorHourlyWebReportViewModel
                                   {
                                       npk = key.npk,
                                       nama = key.nama_operator,
                                       tanggal = key.Date,
                                       id_setting_ro = key.id_ro,
                                       range_waktu = FormatHour(key.Hours),
                                       total_rework = group.Count(x => x.npk == key.npk),
                                       total_waktu_rework = group.Sum(x => Convert.ToInt32(key.jam_akhir.Subtract(key.jam_awal).TotalSeconds)),
                                       id_proses = key.id_proses,
                                       nama_proses = key.nama_proses
                                   });
            #region old query
            //var querySum = (from a in queryTempOpt
            //                join b in queryTempRework
            //                on new { a.npk, a.id_proses, a.tanggal, a.range_waktu, a.id_setting_ro } equals new { b.npk, b.id_proses, b.tanggal, b.range_waktu, b.id_setting_ro }
            //                into finalData
            //                from resultData in finalData.DefaultIfEmpty()
            //                select new RoOperatorHourlyWebReportViewModel
            //                {
            //                    npk = a.npk,
            //                    id_setting_ro = a.id_setting_ro,
            //                    id_proses = a.id_proses,
            //                    tanggal = a.tanggal,
            //                    nama = a.nama,
            //                    nama_proses = a.nama_proses,
            //                    range_waktu = a.range_waktu,
            //                    total_pass = a.total_pass,
            //                    total_rework = finalData.Where(x => x.npk == a.npk && x.id_proses == a.id_proses && x.tanggal == a.tanggal && x.range_waktu == a.range_waktu && x.id_setting_ro == a.id_setting_ro).FirstOrDefault().total_rework,
            //                    total_waktu_rework = finalData.Where(x => x.npk == a.npk && x.id_proses == a.id_proses && x.tanggal == a.tanggal && x.range_waktu == a.range_waktu && x.id_setting_ro == a.id_setting_ro).FirstOrDefault().total_waktu_rework,
            //                }); 
            #endregion

            var querySum = queryTempOpt.Union(queryTempRework);

            querySum.GroupBy(x => new { x.id_proses, x.nama_proses, x.npk, x.tanggal, x.range_waktu, x.nama }, (key, group) =>
                new RoOperatorHourlyWebReportViewModel
                {
                    npk = key.npk,
                    tanggal = key.tanggal,
                    range_waktu = key.range_waktu,
                    total_rework = group.Sum(x=> x.total_rework),
                    total_waktu_rework = group.Sum(x => x.total_waktu_rework),
                    total_pass = group.Sum(x => x.total_pass),
                    id_proses = key.id_proses,
                    nama_proses = key.nama_proses
                });

            return querySum.AsQueryable();
        }

        private IQueryable<RoOperatorHourlyPerDayReportViewModel> GetQueryRoOperatorHourlyPerDay(string unit, int line, string npk)
        {
            var query = (from a in dbContext.TransaksiOperator
                         join b in dbContext.Line
                         on a.id_line equals b.Id
                         where (string.IsNullOrWhiteSpace(unit) || b.nama_unit == unit)
                         && (line == 0 || b.nama_line == line)
                         && (string.IsNullOrWhiteSpace(npk) || a.npk == npk)
                         group new { tgl = a.CreatedUtc.Date } by new { a.CreatedUtc.Date } into G
                         select new RoOperatorHourlyPerDayReportViewModel
                         {
                             tgl = G.Key.Date,
                             qty = G.Count(x => x.tgl.Date == G.Key.Date),
                         });

            return query.AsQueryable();
        }

        private IQueryable<RoOperatorHourlyPerDayPerHourReportViewModel> GetQueryRoOperatorHourlyPerDayPerHour(string unit, int line, string npk, DateTimeOffset? date)
        {
            #region old query
            //var queryTemp = (from a in dbContext.TransaksiOperator
            //                 join b in dbContext.Line
            //                 on a.id_line equals b.Id
            //                 where (string.IsNullOrWhiteSpace(unit) || b.nama_unit == unit)
            //                 && (line == 0 || b.nama_line == line)
            //                 && (string.IsNullOrWhiteSpace(npk) || a.npk == npk)
            //                 && a.pass == true
            //                 && a.CreatedUtc.Date == date.Value.Date
            //                 select new
            //                 {
            //                     a.pass_time
            //                 });

            //var querySum = new List<RoOperatorHourlyPerDayPerHourReportViewModel>();

            //foreach (var i in General.Shift)
            //{
            //    var queryPerHourTemp = (from a in queryTemp
            //                            where a.pass_time >= i.@from
            //                            && a.pass_time <= i.to
            //                            select new RoOperatorHourlyPerDayPerHourReportViewModel
            //                            {
            //                                jam = i.name,
            //                                qty = 1
            //                            })
            //                            .GroupBy(x => x.jam, (key, group) =>
            //                            new RoOperatorHourlyPerDayPerHourReportViewModel
            //                            {
            //                                jam = key,
            //                                qty = group.Sum(x => x.qty)
            //                            });

            //    querySum.AddRange(queryPerHourTemp.ToList());
            //}
            #endregion

            var queryTemp = (from a in dbContext.TransaksiOperator
                             join b in dbContext.Line
                             on a.id_line equals b.Id
                             where (string.IsNullOrWhiteSpace(unit) || b.nama_unit == unit)
                             && (line == 0 || b.nama_line == line)
                             && (string.IsNullOrWhiteSpace(npk) || a.npk == npk)
                             && a.pass == true
                             && a.CreatedUtc.Date == date.Value.Date
                             group a
                             by new { tgl = a.CreatedUtc.Date, jam = a.pass_time.Hours, npk = a.npk } into result
                             select new RoOperatorHourlyPerDayPerHourReportViewModel
                             {
                                 jam = FormatHour(result.Key.jam),
                                 qty = result.Count(x => x.pass)
                             });

            return queryTemp.AsQueryable();
        }

        private IQueryable<RoOperatorHourlyPerDayPerHourPerRoReportViewModel> GetQueryRoOperatorHourlyPerDayPerHourPerRo(string unit, int line, string npk, DateTimeOffset? date, string hour)
        {
            string[] hourSplit = new string[10];
            TimeSpan from = new TimeSpan();
            TimeSpan to = new TimeSpan();
            var Now = DateTime.Now;

            //var hourShift = General.Shift.Find(x => x.name == hour);
            if (!string.IsNullOrWhiteSpace(hour) && hour.Contains("-"))
            {
                hourSplit = hour.Split('-');
                from = TimeSpan.Parse(hourSplit[0].Trim());
                to = TimeSpan.Parse(hourSplit[1].Trim());
            }
            else
            {
                from = new TimeSpan(Now.Hour, Now.Minute, Now.Second);
                to = new TimeSpan(Now.Hour + 1, Now.Minute, Now.Second);
            }
            

            #region query old
            //var queryTemp = (from a in dbContext.TransaksiOperator
            //                 join b in dbContext.Line
            //                 on a.id_line equals b.Id
            //                 where (string.IsNullOrWhiteSpace(unit) || b.nama_unit == unit)
            //                 && (line == 0 || b.nama_line == line)
            //                 && (string.IsNullOrWhiteSpace(npk) || a.npk == npk)
            //                 && (string.IsNullOrWhiteSpace(hour) || (a.pass_time >= hourShift.@from && a.pass_time <= hourShift.to))
            //                 && a.pass == true
            //                 && a.CreatedUtc.Date == date.Value.Date
            //                 select new RoOperatorHourlyPerDayPerHourPerRoReportViewModel
            //                 {
            //                     ro = a.rono,
            //                     qty = 1
            //                 });

            //var querySum = queryTemp
            //               .GroupBy(x => x.ro, (key, group) =>
            //               new RoOperatorHourlyPerDayPerHourPerRoReportViewModel
            //               {
            //                   ro = key,
            //                   qty = group.Sum(x => x.qty)
            //               });
            #endregion

            var queryTemp = (from a in dbContext.TransaksiOperator
                             join b in dbContext.Line
                             on a.id_line equals b.Id
                             where (string.IsNullOrWhiteSpace(unit) || b.nama_unit == unit)
                             && (line == 0 || b.nama_line == line)
                             && (string.IsNullOrWhiteSpace(npk) || a.npk == npk)
                             && (string.IsNullOrWhiteSpace(hour) || (a.pass_time >= @from && a.pass_time <= @to))
                             && a.pass == true
                             && a.CreatedUtc.Date == date.Value.Date
                             select new RoOperatorHourlyPerDayPerHourPerRoReportViewModel
                             {
                                 ro = a.rono,
                                 qty = 1
                             });

            var querySum = queryTemp
                           .GroupBy(x => x.ro, (key, group) =>
                           new RoOperatorHourlyPerDayPerHourPerRoReportViewModel
                           {
                               ro = key,
                               qty = group.Sum(x => x.qty)
                           });

            return querySum.AsQueryable();
        }

        private RoOperatorHourlyDataReportViewModel GetQueryRoOperatorHourlyReport(string unit, int line, string npk, DateTimeOffset? date, string hour, string ro)
        {
            string[] hourSplit = new string[10];
            TimeSpan from = new TimeSpan();
            TimeSpan to = new TimeSpan();
            var Now = DateTime.Now;

            //var hourShift = General.Shift.Find(x => x.name == hour);
            if (!string.IsNullOrWhiteSpace(hour) && hour.Contains("-"))
            {
                hourSplit = hour.Split('-');
                from = TimeSpan.Parse(hourSplit[0].Trim());
                to = TimeSpan.Parse(hourSplit[1].Trim());
            }
            else
            {
                from = new TimeSpan(Now.Hour, Now.Minute, Now.Second);
                to = new TimeSpan(Now.Hour + 1, Now.Minute, Now.Second);
            }

            #region query old
            //var queryTemp = (from a in dbContext.TransaksiOperator
            //                 join b in dbContext.Line
            //                 on a.id_line equals b.Id
            //                 where (string.IsNullOrWhiteSpace(unit) || b.nama_unit == unit)
            //                 && (line == 0 || b.nama_line == line)
            //                 && (string.IsNullOrWhiteSpace(npk) || a.npk == npk)
            //                 && (string.IsNullOrWhiteSpace(hour) || (a.pass_time >= hourShift.@from && a.pass_time <= hourShift.to))
            //                 && (string.IsNullOrWhiteSpace(ro) || a.rono == ro)
            //                 && a.pass == true
            //                 && a.CreatedUtc.Date == date.Value.Date
            //                 select new RoOperatorHourlyReportViewModel
            //                 {
            //                     waktu = a.pass_time,
            //                     proses = a.nama_proses,
            //                     pass = 1
            //                 });
            #endregion

            var queryTemp = (from a in dbContext.TransaksiOperator
                             join b in dbContext.Line
                             on a.id_line equals b.Id
                             where (string.IsNullOrWhiteSpace(unit) || b.nama_unit == unit)
                             && (line == 0 || b.nama_line == line)
                             && (string.IsNullOrWhiteSpace(npk) || a.npk == npk)
                             && (string.IsNullOrWhiteSpace(hour) || (a.pass_time >= @from && a.pass_time <= @to))
                             && (string.IsNullOrWhiteSpace(ro) || a.rono == ro)
                             && a.pass == true
                             && a.CreatedUtc.Date == date.Value.Date
                             select new RoOperatorHourlyReportViewModel
                             {
                                 waktu = a.pass_time,
                                 proses = a.nama_proses,
                                 pass = 1
                             });

            var queryGroupTemp = queryTemp
                                .GroupBy(x => new { x.proses, x.waktu, x.pass }, (key, group) =>
                                new RoOperatorHourlyReportViewModel
                                {
                                    waktu = key.waktu,
                                    proses = key.proses,
                                    pass = key.pass
                                }).ToList();

            var queryReworkTemp = (from a in dbContext.Rework
                                   join b in dbContext.ReworkTime
                                   on a.Id equals b.id_rework
                                   join c in dbContext.Line
                                   on a.id_line equals c.Id
                                   where (string.IsNullOrWhiteSpace(unit) || c.nama_unit == unit)
                                   && (line == 0 || c.nama_line == line)
                                   && (string.IsNullOrWhiteSpace(npk) || a.npk == npk)
                                   && (string.IsNullOrWhiteSpace(ro) || a.rono == ro)
                                   //&& a.CreatedUtc.Date == date.Value.Date
                                   select new RoOperatorHourlyReworkReportViewModel
                                   {
                                       tgl = b.CreatedUtc.Date,
                                       proses = a.nama_proses,
                                       waktu = (b.jam_akhir.Subtract(b.jam_awal)).TotalSeconds.ToString(),
                                       rework = 1
                                   }).ToList();

            var query = new RoOperatorHourlyDataReportViewModel()
            {
                data = queryGroupTemp,
                dataRework = queryReworkTemp
            };

            return query;
        }

        private RoOperatorHourlyExcelReportViewModel GetQueryRoOperatorHourlyExcelReport(string unit, int line, DateTimeOffset? date, string ro)
        {
            var queryTemp = (from a in dbContext.TransaksiOperator
                             join b in dbContext.Line
                             on a.id_line equals b.Id
                             where (string.IsNullOrWhiteSpace(unit) || b.nama_unit == unit)
                             && (line == 0 || b.nama_line == line)
                             && (string.IsNullOrWhiteSpace(ro) || a.rono == ro)
                             && a.pass == true
                             && a.CreatedUtc.Date == date.Value.Date
                             select new RoOperatorHourlyExcelViewModel
                             {
                                 nama = a.nama,
                                 ro = a.rono,
                                 tgl = a.CreatedUtc.Date,
                                 shift = FormatHour(a.pass_time.Hours),
                                 jam = a.pass_time,
                                 proses = a.nama_proses,
                                 pass = 1
                             }).ToList();

            var queryReworkTemp = (from a in dbContext.Rework
                                   join b in dbContext.ReworkTime
                                   on a.Id equals b.id_rework
                                   join c in dbContext.Line
                                   on a.id_line equals c.Id
                                   where (string.IsNullOrWhiteSpace(unit) || c.nama_unit == unit)
                                   && (line == 0 || c.nama_line == line)
                                   && (string.IsNullOrWhiteSpace(ro) || a.rono == ro)
                                   //&& a.CreatedUtc.Date == date.Value.Date
                                   select new RoOperatorHourlyReworkExcelViewModel
                                   {
                                       nama = a.nama_operator,
                                       ro = a.rono,
                                       tgl = a.CreatedUtc.Date,
                                       shift = FormatHour(b.jam_awal.Hours),
                                       jam = (b.jam_akhir.Subtract(b.jam_awal)).TotalSeconds.ToString(),
                                       proses = a.nama_proses,
                                   }).ToList();

            var query = new RoOperatorHourlyExcelReportViewModel()
            {
                data = queryTemp,
                dataRework = queryReworkTemp
            };

            return query;

        }
        #endregion

        #region utils
        private string FormatHour(int hour)
        {
            string convertHour = hour.ToString();
            if (hour < 10)
            {
                convertHour = $"0{convertHour}:00 - {hour + 1}:00";
            }
            else
            {
                convertHour = $"{convertHour}:00 - {hour + 1}:00";
            }

            return convertHour;
        }

        private DataTable TransposeTable(DataTable dt)
        {
            DataTable dtn = new DataTable();

            //adding columns    
            for (int i = 0; i <= dt.Rows.Count; i++)
            {
                dtn.Columns.Add(i.ToString());
            }

            //Changing Column Captions: 
            dtn.Columns[0].ColumnName = "Tanggal";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //For dateTime columns use like below
                dtn.Columns[i + 1].ColumnName = Convert.ToDateTime(dt.Rows[i].ItemArray[0].ToString()).ToString("MM/dd/yyyy");
                //Else just assign the ItermArry[0] to the columnName prooperty
            }

            //Adding Row Data
            for (int k = 1; k < dt.Columns.Count; k++)
            {
                DataRow r = dtn.NewRow();
                r[0] = dt.Columns[k].ToString();
                for (int j = 1; j <= dt.Rows.Count; j++)
                    r[j] = dt.Rows[j - 1][k];
                dtn.Rows.Add(r);
            }

            return dtn;
        }
        #endregion

        #region facade
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

        public MemoryStream GetRoDetailOptExcel(string ro, string unit, int line, DateTimeOffset? date)
        {
            var data = GetQueryRoDetailOpt(ro, unit, line, date);

            DataTable result = new DataTable();
            DataTable resultTranspose = new DataTable();

            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Data", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "RO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Pass", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Reject", DataType = typeof(Double) });

            if (data.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", 0, 0); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in data)
                {
                    index++;
                    result.Rows.Add(item.tgl.Date, index, item.rono, item.nama, item.pass, item.reject);
                }
            }

            ExcelPackage package = new ExcelPackage();
            CultureInfo Id = new CultureInfo("id-ID");

            var sheet = package.Workbook.Worksheets.Add("Report");

            resultTranspose = TransposeTable(result);

            var col = (char)('A' + result.Columns.Count);
            string tgl = date.Value.ToString("dd MMM yyyy", Id);

            sheet.Cells[$"A1:{col}1"].Value = string.Format("LAPORAN DETAIL OPT {0}", line == 0 ? "ALL LINE" : $"LINE {line}");
            sheet.Cells[$"A1:{col}1"].Merge = true;
            sheet.Cells[$"A1:{col}1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A1:{col}1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells[$"A1:{col}1"].Style.Font.Bold = true;
            sheet.Cells[$"A2:{col}2"].Value = string.Format("Tanggal {0}", tgl);
            sheet.Cells[$"A2:{col}2"].Merge = true;
            sheet.Cells[$"A2:{col}2"].Style.Font.Bold = true;
            sheet.Cells[$"A2:{col}2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A2:{col}2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells[$"A3:{col}3"].Value = string.Format("KONFEKSI : {0}", unit);
            sheet.Cells[$"A3:{col}3"].Merge = true;
            sheet.Cells[$"A3:{col}3"].Style.Font.Bold = true;
            sheet.Cells[$"A3:{col}3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A3:{col}3"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells["A5"].LoadFromDataTable(resultTranspose, true, OfficeOpenXml.Table.TableStyles.Light16);

            MemoryStream stream = new MemoryStream();
            package.SaveAs(stream);
            return stream;
        }

        public Tuple<List<RoDetailOptReportViewModel>, int> GetRoDetailOptReport(string ro, string unit, int line, DateTimeOffset? date)
        {
            var data = GetQueryRoDetailOpt(ro, unit, line, date);

            return Tuple.Create(data.ToList(), data.Count());
        }

        public MemoryStream GetRoOperatorHourlyWebExcel(string area, int line, string proses, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            var data = GetQueryRoOperatorHourlyWeb(area, line, proses, dateFrom, dateTo);

            DataTable result = new DataTable();

            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Proses", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Range Waktu", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Total Pass", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Total Waktu Rework", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Total Rework", DataType = typeof(Double) });

            if (data.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", 0, 0, 0); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in data)
                {
                    index++;
                    result.Rows.Add(item.tanggal.ToString("dd-MM-yyyy"), item.nama, item.nama_proses, item.range_waktu, item.total_pass, item.total_waktu_rework, item.total_rework);
                }
            }

            ExcelPackage package = new ExcelPackage();
            CultureInfo Id = new CultureInfo("id-ID");

            var sheet = package.Workbook.Worksheets.Add("Report");

            var col = (char)('A' + result.Columns.Count);
            string tgla = dateFrom.Value.ToString("dd MMM yyyy", Id);
            string tglb = dateTo.Value.ToString("dd MMM yyyy", Id);

            sheet.Cells[$"A1:{col}1"].Value = string.Format("LAPORAN DETAIL OPT {0}", line == 0 ? "ALL LINE" : $"LINE {line}");
            sheet.Cells[$"A1:{col}1"].Merge = true;
            sheet.Cells[$"A1:{col}1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A1:{col}1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells[$"A1:{col}1"].Style.Font.Bold = true;
            sheet.Cells[$"A2:{col}2"].Value = string.Format("Tanggal {0} - {1}", tgla, tglb);
            sheet.Cells[$"A2:{col}2"].Merge = true;
            sheet.Cells[$"A2:{col}2"].Style.Font.Bold = true;
            sheet.Cells[$"A2:{col}2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A2:{col}2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells[$"A3:{col}3"].Merge = true;
            sheet.Cells[$"A3:{col}3"].Style.Font.Bold = true;
            sheet.Cells[$"A3:{col}3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A3:{col}3"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells["A5"].LoadFromDataTable(result, true, OfficeOpenXml.Table.TableStyles.Light16);

            MemoryStream stream = new MemoryStream();
            package.SaveAs(stream);
            return stream;

        }

        public Tuple<List<RoOperatorHourlyWebReportViewModel>, int> GetRoOperatorHourlyWebReport(string area, int line, string proses, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            var data = GetQueryRoOperatorHourlyWeb(area, line, proses, dateFrom, dateTo);

            return Tuple.Create(data.ToList(), data.Count());
        }

        public MemoryStream GetRoOperatorHourlyExcel(string unit, int line, DateTimeOffset? date, string ro)
        {
            var data = GetQueryRoOperatorHourlyExcelReport(unit, line, date, ro);
            var opt = data.data.Select(x => x.nama).Distinct().ToList();
            ExcelPackage package = new ExcelPackage();

            if (opt.Count() > 0)
            {
                foreach (var i in opt)
                {
                    var sheet = package.Workbook.Worksheets.Add(i);

                    #region proses
                    DataTable proses = new DataTable();
                    proses.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(String) });
                    proses.Columns.Add(new DataColumn() { ColumnName = "Shift", DataType = typeof(String) });
                    proses.Columns.Add(new DataColumn() { ColumnName = "Jam", DataType = typeof(String) });
                    proses.Columns.Add(new DataColumn() { ColumnName = "RO", DataType = typeof(String) });
                    proses.Columns.Add(new DataColumn() { ColumnName = "Proses", DataType = typeof(String) });

                    //var col = (char)('A' + proses.Columns.Count);
                    int index = 0;

                    if (data.data.ToArray().Count() == 0)
                        proses.Rows.Add("", "", "", "", ""); // to allow column name to be generated properly for empty data as template
                    else
                    {
                        foreach (var item in data.data)
                        {
                            index++;
                            proses.Rows.Add(item.tgl.ToString("dd-MM-yyyy"), item.shift, item.jam, item.ro, item.proses);
                        }
                    }

                    sheet.Cells["A5"].LoadFromDataTable(proses, true, OfficeOpenXml.Table.TableStyles.Light16);
                    #endregion

                    #region rework
                    DataTable rework = new DataTable();
                    rework.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(String) });
                    rework.Columns.Add(new DataColumn() { ColumnName = "Waktu", DataType = typeof(String) });
                    rework.Columns.Add(new DataColumn() { ColumnName = "Durasi", DataType = typeof(String) });
                    rework.Columns.Add(new DataColumn() { ColumnName = "RO", DataType = typeof(String) });

                    //var colr = (char)('A' + rework.Columns.Count);

                    if (data.dataRework.ToArray().Count() == 0)
                        rework.Rows.Add("", "", "", ""); // to allow column name to be generated properly for empty data as template
                    else
                    {
                        foreach (var item in data.dataRework)
                        {
                            rework.Rows.Add(item.tgl.ToString("dd-MM-yyyy"), item.shift, item.jam, item.ro);
                        }
                    }
                    var start = (string)("A" + (8 + index));
                    sheet.Cells[start].LoadFromDataTable(rework, true, OfficeOpenXml.Table.TableStyles.Light16);
                    #endregion

                }
            }
            else
            {
                var sheet = package.Workbook.Worksheets.Add("report");

                #region proses
                DataTable proses = new DataTable();
                proses.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(String) });
                proses.Columns.Add(new DataColumn() { ColumnName = "Shift", DataType = typeof(String) });
                proses.Columns.Add(new DataColumn() { ColumnName = "Jam", DataType = typeof(String) });
                proses.Columns.Add(new DataColumn() { ColumnName = "RO", DataType = typeof(String) });
                proses.Columns.Add(new DataColumn() { ColumnName = "Proses", DataType = typeof(String) });

                proses.Rows.Add("", "", "", "", ""); // to allow column name to be generated properly for empty data as template
                sheet.Cells["A5"].LoadFromDataTable(proses, true, OfficeOpenXml.Table.TableStyles.Light16);
                #endregion

                #region rework
                DataTable rework = new DataTable();
                rework.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(String) });
                rework.Columns.Add(new DataColumn() { ColumnName = "Waktu", DataType = typeof(String) });
                rework.Columns.Add(new DataColumn() { ColumnName = "Durasi", DataType = typeof(String) });
                rework.Columns.Add(new DataColumn() { ColumnName = "RO", DataType = typeof(String) });

                rework.Rows.Add("", "", "", ""); // to allow column name to be generated properly for empty data as template
                var start = (string)("A8");
                sheet.Cells[start].LoadFromDataTable(rework, true, OfficeOpenXml.Table.TableStyles.Light16);
                #endregion

            }

            MemoryStream stream = new MemoryStream();
            package.SaveAs(stream);
            return stream;
        }

        public Tuple<List<RoOperatorHourlyPerDayReportViewModel>, int> GetRoOperatorHourlyPerDayReport(string unit, int line, string npk)
        {
            var data = GetQueryRoOperatorHourlyPerDay(unit, line, npk);

            return Tuple.Create(data.ToList(), data.Count());
        }

        public Tuple<List<RoOperatorHourlyPerDayPerHourReportViewModel>, int> GetRoOperatorHourlyPerDayPerHourReport(string unit, int line, string npk, DateTimeOffset? date)
        {
            var data = GetQueryRoOperatorHourlyPerDayPerHour(unit, line, npk, date);

            return Tuple.Create(data.ToList(), data.Count());
        }

        public Tuple<List<RoOperatorHourlyPerDayPerHourPerRoReportViewModel>, int> GetRoOperatorHourlyPerDayPerHourPerRoReport(string unit, int line, string npk, DateTimeOffset? date, string hour)
        {
            var data = GetQueryRoOperatorHourlyPerDayPerHourPerRo(unit, line, npk, date, hour);

            return Tuple.Create(data.ToList(), data.Count());
        }

        public RoOperatorHourlyDataReportViewModel GetRoOperatorHourlyReport(string unit, int line, string npk, DateTimeOffset? date, string hour, string ro)
        {
            var data = GetQueryRoOperatorHourlyReport(unit, line, npk, date, hour, ro);

            return data;
        }

        #endregion
    }
}
