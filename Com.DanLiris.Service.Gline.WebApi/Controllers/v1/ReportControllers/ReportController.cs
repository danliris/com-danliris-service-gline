using Com.DanLiris.Service.Gline.Lib.Interfaces;
using Com.DanLiris.Service.Gline.Lib.Services;
using Com.DanLiris.Service.Gline.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Gline.WebApi.Controllers.v1.ReportControllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/report")]
    [Authorize]
    public class ReportController : Controller
    {
        private readonly string ApiVersion = "1.0.0";
        private readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;
        private readonly IReportFacade facade;

        public ReportController(IServiceProvider serviceProvider, IReportFacade facade)
        {
            this.serviceProvider = serviceProvider;
            this.facade = facade;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
        }

        [HttpGet("ro-done")]
        public IActionResult GetRoDoneReport(string unit, int line, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            try
            {
                if (dateTo == null)
                    dateTo = DateTimeOffset.UtcNow;

                if (dateFrom == null)
                    dateFrom = DateTimeOffset.MinValue;

                var data = facade.GetRoDoneReport(unit, line, dateFrom.GetValueOrDefault(), dateTo.GetValueOrDefault());

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = data.Item1,
                    info = new { total = data.Item2 },
                    message = General.OK_MESSAGE,
                    statusCode = General.OK_STATUS_CODE
                });
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("ro-done/download")]
        public IActionResult GetRoDoneXls(string unit, int line, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            try
            {
                if (dateTo == null)
                    dateTo = DateTimeOffset.UtcNow;

                if (dateFrom == null)
                    dateFrom = DateTimeOffset.MinValue;

                byte[] xlsInBytes;
                var xls = facade.GetRoDoneExcel(unit, line, dateFrom.GetValueOrDefault(), dateTo.GetValueOrDefault());

                string filename = "Laporan Ro Done";
                if (dateFrom != null) filename += " " + ((DateTime)dateFrom.Value.DateTime).ToString("dd-MM-yyyy");
                if (dateTo != null) filename += "_" + ((DateTime)dateTo.Value.DateTime).ToString("dd-MM-yyyy");
                filename += ".xlsx";

                xlsInBytes = xls.ToArray();
                var file = File(xlsInBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
                return file;
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("ro-hourly")]
        public IActionResult GetRoHourlyReport(string unit, int line, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            try
            {
                if (dateTo == null)
                    dateTo = DateTimeOffset.UtcNow;

                if (dateFrom == null)
                    dateFrom = DateTimeOffset.MinValue;

                var data = facade.GetRoHourlyReport(unit, line, dateFrom.GetValueOrDefault(), dateTo.GetValueOrDefault());

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = data.Item1,
                    info = new { total = data.Item2 },
                    message = General.OK_MESSAGE,
                    statusCode = General.OK_STATUS_CODE
                });
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("ro-hourly/download")]
        public IActionResult GetRoHourlyXls(string unit, int line, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            try
            {
                if (dateTo == null)
                    dateTo = DateTimeOffset.UtcNow;

                if (dateFrom == null)
                    dateFrom = DateTimeOffset.MinValue;

                byte[] xlsInBytes;
                var xls = facade.GetRoHourlyExcel(unit, line, dateFrom.GetValueOrDefault(), dateTo.GetValueOrDefault());

                string filename = "Laporan Per Jam Ro";
                if (dateFrom != null) filename += " " + ((DateTime)dateFrom.Value.DateTime).ToString("dd-MM-yyyy");
                if (dateTo != null) filename += "_" + ((DateTime)dateTo.Value.DateTime).ToString("dd-MM-yyyy");
                filename += ".xlsx";

                xlsInBytes = xls.ToArray();
                var file = File(xlsInBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
                return file;
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("ro-detail-opt")]
        public IActionResult GetRoDetailOptReport(string ro, string unit, int line, DateTimeOffset? date)
        {
            try
            {
                if (date == null)
                    date = DateTimeOffset.UtcNow;

                var data = facade.GetRoDetailOptReport(ro, unit, line, date.GetValueOrDefault());

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = data.Item1,
                    info = new { total = data.Item2 },
                    message = General.OK_MESSAGE,
                    statusCode = General.OK_STATUS_CODE
                });
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("ro-detail-opt/download")]
        public IActionResult GetRoDetailOptXls(string ro, string unit, int line, DateTimeOffset? date)
        {
            try
            {
                if (date == null)
                    date = DateTimeOffset.UtcNow;

                byte[] xlsInBytes;
                var xls = facade.GetRoDetailOptExcel(ro, unit, line, date.GetValueOrDefault());

                string filename = "Laporan Detail Operator";
                if (!string.IsNullOrWhiteSpace(unit)) filename += " " + unit;
                if (date != null) filename += " " + ((DateTime)date.Value.DateTime).ToString("dd-MM-yyyy");
                filename += ".xlsx";

                xlsInBytes = xls.ToArray();
                var file = File(xlsInBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
                return file;
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("ro-operator-hourly/web")]
        public IActionResult GetRoOperatorHourlyWebReport(string area, int line, string proses, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            try
            {
                if (dateTo == null)
                    dateTo = DateTimeOffset.UtcNow;

                if (dateFrom == null)
                    dateFrom = DateTimeOffset.MinValue;

                var data = facade.GetRoOperatorHourlyWebReport(area, line, proses, dateFrom.GetValueOrDefault(), dateTo.GetValueOrDefault());

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = data.Item1,
                    info = new { total = data.Item2 },
                    message = General.OK_MESSAGE,
                    statusCode = General.OK_STATUS_CODE
                });
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                   new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                   .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }

        }

        [HttpGet("ro-operator-hourly/web/download")]
        public IActionResult GetRoOperatorHourlyWebXls(string area, int line, string proses, DateTimeOffset? dateFrom, DateTimeOffset? dateTo) 
        {
            try
            {
                if (dateTo == null)
                    dateTo = DateTimeOffset.UtcNow;

                if (dateFrom == null)
                    dateFrom = DateTimeOffset.MinValue;

                byte[] xlsInBytes;
                var xls = facade.GetRoOperatorHourlyWebExcel(area, line, proses, dateFrom.GetValueOrDefault(), dateTo.GetValueOrDefault());

                string filename = "Laporan Per Jam Operator";
                if (line != 0) filename += " " + line;
                if (dateFrom != null) filename += " " + ((DateTime)dateFrom.Value.DateTime).ToString("dd-MM-yyyy");
                if (dateTo != null) filename += " " + ((DateTime)dateTo.Value.DateTime).ToString("dd-MM-yyyy");
                filename += ".xlsx";

                xlsInBytes = xls.ToArray();
                var file = File(xlsInBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
                return file;
            }
            
            catch(Exception e)
            {
                Dictionary<string, object> Result =
                  new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                  .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("ro-operator-hourly/per-day")]
        public IActionResult GetRoOperatorHourlyPerDayReport(string unit, int line, string npk)
        {
            try
            {
                var data = facade.GetRoOperatorHourlyPerDayReport(unit, line, npk);

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = data.Item1,
                    info = new { total = data.Item2 },
                    message = General.OK_MESSAGE,
                    statusCode = General.OK_STATUS_CODE
                });
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("ro-operator-hourly/per-hour")]
        public IActionResult GetRoOperatorHourlyPerHourReport(string unit, int line, string npk, DateTimeOffset? date)
        {
            try
            {
                if (date == null)
                    date = DateTimeOffset.UtcNow;

                var data = facade.GetRoOperatorHourlyPerDayPerHourReport(unit, line, npk, date);

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = data.Item1,
                    info = new { total = data.Item2 },
                    message = General.OK_MESSAGE,
                    statusCode = General.OK_STATUS_CODE
                });
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("ro-operator-hourly/per-ro")]
        public IActionResult GetRoOperatorHourlyPerRoReport(string unit, int line, string npk, DateTimeOffset? date, string hour)
        {
            try
            {
                if (date == null)
                    date = DateTimeOffset.UtcNow;

                var data = facade.GetRoOperatorHourlyPerDayPerHourPerRoReport(unit, line, npk, date, hour);

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = data.Item1,
                    info = new { total = data.Item2 },
                    message = General.OK_MESSAGE,
                    statusCode = General.OK_STATUS_CODE
                });
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("ro-operator-hourly")]
        public IActionResult GetRoOperatorHourlyReport(string unit, int line, string npk, DateTimeOffset? date, string hour, string ro)
        {
            try
            {
                if (date == null)
                    date = DateTimeOffset.UtcNow;

                var data = facade.GetRoOperatorHourlyReport(unit, line, npk, date, hour, ro);

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = data,
                    message = General.OK_MESSAGE,
                    statusCode = General.OK_STATUS_CODE
                });
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("ro-operator-hourly/download")]
        public IActionResult GetRoOperatorHourlyXls(string unit, int line, DateTimeOffset? date, string ro)
        {
            try
            {
                if (date == null)
                    date = DateTimeOffset.UtcNow;

                byte[] xlsInBytes;
                var xls = facade.GetRoOperatorHourlyExcel(unit, line, date.GetValueOrDefault(), ro);

                string filename = "Laporan Per Jam Operator";
                if (!string.IsNullOrWhiteSpace(unit)) filename += " " + unit;
                if (date != null) filename += " " + ((DateTime)date.Value.DateTime).ToString("dd-MM-yyyy");
                filename += ".xlsx";

                xlsInBytes = xls.ToArray();
                var file = File(xlsInBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
                return file;
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }
    }
}
