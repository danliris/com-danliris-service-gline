using AutoMapper;
using Com.DanLiris.Service.Gline.Lib.Interfaces;
using Com.DanLiris.Service.Gline.Lib.Models.MasterModel;
using Com.DanLiris.Service.Gline.Lib.Services;
using Com.DanLiris.Service.Gline.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Gline.Lib.ViewModels.MasterViewModel;
using Com.DanLiris.Service.Gline.WebApi.Helpers;
using Com.Moonlay.NetCore.Lib.Service;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Gline.WebApi.Controllers.v1.LineControllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/line")]
    [Authorize]
    public class LineController : Controller
    {
        private readonly string ApiVersion = "1.0.0";

        private readonly IdentityService _identityService;
        private readonly IMapper _mapper;
        private readonly ILineFacade _facade;
        private readonly IValidateService _validateService;

        private readonly string ContentType = "application/vnd.openxmlformats";
        private readonly string FileName = string.Concat("Error Log - Upload Line - ", DateTime.Now.ToString("dd MMM yyyy"), ".csv");

        public LineController(IServiceProvider serviceProvider, ILineFacade facade, IMapper mapper)
        {
            _mapper = mapper;
            _facade = facade;
            _identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
            _validateService = (IValidateService)serviceProvider.GetService(typeof(IValidateService));
        }

        private void VerifyUser()
        {
            _identityService.Username = User.Claims.ToArray().SingleOrDefault(p => p.Type.Equals("username")).Value;
            _identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
            _identityService.TimezoneOffset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
        }

        [HttpGet]
        public IActionResult Get(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}")
        {
            try
            {
                var Data = _facade.Read(page, size, order, keyword, filter);
                var newData = _mapper.Map<List<LineViewModel>>(Data.Item1);

                List<object> listData = new List<object>();
                listData.AddRange(newData.AsQueryable().Select(s => new
                {
                    s.Id,
                    s.nama_line,
                    s.nama_gedung,
                    s.kode_unit,
                    s.nama_unit,
                    s.CreatedAgent,
                    s.CreatedBy,
                    s.CreatedUtc,
                    s.LastModifiedAgent,
                    s.LastModifiedBy,
                    s.LastModifiedUtc
                }));

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    statusCode = General.OK_STATUS_CODE,
                    message = General.OK_MESSAGE,
                    data = listData,
                    info = new Dictionary<string, object>
                    {
                        { "count", listData.Count },
                        { "total", Data.Item2 },
                        { "order", Data.Item3 },
                        { "page", page },
                        { "size", size }
                    },
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

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out Guid parseResult))
                    return NotFound();

                Guid guid = Guid.Parse(id);

                var result = _facade.ReadById(guid);
                LineViewModel viewModel = _mapper.Map<LineViewModel>(result);
                if (viewModel == null)
                {
                    Dictionary<string, object> Result =
                          new ResultFormatter(ApiVersion, General.NOT_FOUND_STATUS_CODE, General.NOT_FOUND_MESSAGE)
                          .Fail();

                    return NotFound();
                }

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    statusCode = General.OK_STATUS_CODE,
                    message = General.OK_MESSAGE,
                    data = viewModel,
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

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LineCreateModel createModel)
        {
            VerifyUser();

            try
            {
                _validateService.Validate(createModel);

                Line model = _mapper.Map<Line>(createModel);

                int result = await _facade.Create(model, _identityService.Username);

                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.CREATED_STATUS_CODE, General.OK_MESSAGE)
                    .Ok();
                return Created(String.Concat(Request.Path, "/", 0), Result);
            }
            catch (ServiceValidationExeption e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.BAD_REQUEST_STATUS_CODE, General.BAD_REQUEST_MESSAGE)
                    .Fail(e);
                return BadRequest(Result);
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] string id, [FromBody] LineViewModel viewModel)
        {
            VerifyUser();

            try
            {
                _validateService.Validate(viewModel);

                if (!Guid.TryParse(id, out Guid parseResult))
                    return NotFound();

                Guid guid = Guid.Parse(id);

                Line m = _mapper.Map<Line>(viewModel);

                int result = await _facade.Update(guid, m, _identityService.Username);

                return NoContent();
            }
            catch (ServiceValidationExeption e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.BAD_REQUEST_STATUS_CODE, General.BAD_REQUEST_MESSAGE)
                    .Fail(e);
                return BadRequest(Result);

            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }

        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] string id)
        {
            VerifyUser();

            try
            {
                if (!Guid.TryParse(id, out Guid parseResult))
                    return NotFound();

                Guid guid = Guid.Parse(id);

                _facade.Delete(guid, _identityService.Username);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE);
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> PostCSVFileAsync()
        {
            _identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

            try
            {
                if (Request.Form.Files.Count > 0)
                {
                    var UploadedFile = Request.Form.Files[0];
                    StreamReader Reader = new StreamReader(UploadedFile.OpenReadStream());
                    List<string> FileHeader = new List<string>(Reader.ReadLine().Replace("\"", string.Empty).Split(","));
                    var ValidHeader = _facade.CsvHeader.SequenceEqual(FileHeader, StringComparer.OrdinalIgnoreCase);

                    if (ValidHeader)
                    {
                        Reader.DiscardBufferedData();
                        Reader.BaseStream.Seek(0, SeekOrigin.Begin);
                        Reader.BaseStream.Position = 0;
                        CsvReader Csv = new CsvReader(Reader);
                        Csv.Configuration.IgnoreQuotes = false;
                        Csv.Configuration.Delimiter = ",";
                        Csv.Configuration.RegisterClassMap<Lib.Facades.LineFacades.LineFacade.LineMap>();
                        Csv.Configuration.HeaderValidated = null;

                        List<LineCsvViewModel> viewModelCsv = Csv.GetRecords<LineCsvViewModel>().ToList();
                        Tuple<bool, List<object>> Validated = _facade.UploadValidate(ref viewModelCsv, Request.Form.ToList());

                        Reader.Close();

                        if (Validated.Item1)
                        {

                            List<LineViewModel> viewModel = await _facade.MapCsvToViewModel(viewModelCsv);
                            List<Line> model = _mapper.Map<List<Line>>(viewModel);
                            await _facade.UploadData(model, _identityService.Username);

                            Dictionary<string, object> Result =
                               new ResultFormatter(ApiVersion, General.CREATED_STATUS_CODE, General.OK_MESSAGE)
                               .Ok();

                            return Created(HttpContext.Request.Path, Result);
                        }
                        else
                        {
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                using (StreamWriter streamWriter = new StreamWriter(memoryStream))
                                {
                                    using (CsvWriter csvWriter = new CsvWriter(streamWriter))
                                    {
                                        csvWriter.WriteRecords(Validated.Item2);
                                    }

                                    return File(memoryStream.ToArray(), ContentType, FileName);
                                }
                            }
                        }
                    }
                    else
                    {
                        Dictionary<string, object> Result =
                          new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, General.CSV_ERROR_MESSAGE)
                          .Fail();

                        return NotFound(Result);
                    }
                }
                else
                {
                    Dictionary<string, object> Result =
                        new ResultFormatter(ApiVersion, General.BAD_REQUEST_STATUS_CODE, General.NO_FILE_ERROR_MESSAGE)
                            .Fail();
                    return BadRequest(Result);
                }
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
