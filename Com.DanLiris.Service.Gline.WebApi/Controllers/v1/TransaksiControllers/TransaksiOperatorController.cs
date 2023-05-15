using AutoMapper;
using Com.DanLiris.Service.Gline.Lib.Interfaces;
using Com.DanLiris.Service.Gline.Lib.Models.ReworkModel;
using Com.DanLiris.Service.Gline.Lib.Models.TransaksiModel;
using Com.DanLiris.Service.Gline.Lib.Services;
using Com.DanLiris.Service.Gline.Lib.ViewModels.ReworkViewModel;
using Com.DanLiris.Service.Gline.Lib.ViewModels.TransaksiViewModel;
using Com.DanLiris.Service.Gline.WebApi.Helpers;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Gline.WebApi.Controllers.v1.TransaksiControllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/operator")]
    [Authorize]
    public class TransaksiOperatorController : Controller
    {
        private readonly string ApiVersion = "1.0.0";

        private readonly IdentityService _identityService;
        private readonly IMapper _mapper;
        private readonly ITransaksiOperatorFacade _facade;
        private readonly IValidateService _validateService;

        public TransaksiOperatorController(IServiceProvider serviceProvider, ITransaksiOperatorFacade facade, IMapper mapper)
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
                var newData = _mapper.Map<List<TransaksiOperatorViewModel>>(Data.Item1);

                List<object> listData = new List<object>();
                listData.AddRange(newData.AsQueryable().Select(s => new
                {
                    s.Id,
                    s.npk,
                    s.nama,
                    s.nama_line,
                    s.rono,
                    s.quantity,
                    s.nama_proses,
                    s.pass,
                    s.pass_time,
                    s.nama_shift
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

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TransaksiOperatorCreateModel createModel)
        {
            VerifyUser();

            try
            {
                _validateService.Validate(createModel);

                TransaksiOperator model = _mapper.Map<TransaksiOperator>(createModel);

                var result = await _facade.Create(model, _identityService.Username);

                if (result.roOverflow == true)
                {
                    Dictionary<string, object> ErrorResult =
                        new ResultFormatter(ApiVersion, General.BAD_REQUEST_STATUS_CODE, General.QUANTITY_OVERFLOW)
                        .Fail();
                    return BadRequest(ErrorResult);
                }

                //Dictionary<string, object> Result =
                //    new ResultFormatter(ApiVersion, General.CREATED_STATUS_CODE, General.OK_MESSAGE)
                //    .Ok();
                return CreatedAtAction(nameof(Get), result);
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

        [HttpPost("rework")]
        public async Task<IActionResult> AddRework([FromBody] ReworkTimeCreateModel createModel, [Required] string npk, [Required] Guid id_ro, [Required] Guid id_line, [Required] Guid id_proses)
        {
            VerifyUser();

            try
            {
                ReworkTime model = new ReworkTime
                {
                    jam_awal = createModel.jam_awal,
                    jam_akhir = createModel.jam_akhir
                };

                int result = await _facade.DoRework(model, _identityService.Username, npk, id_ro, id_line, id_proses);

                if (result == -1)
                {
                    Dictionary<string, object> ErrorResult =
                        new ResultFormatter(ApiVersion, General.NOT_FOUND_STATUS_CODE, General.NOT_FOUND_MESSAGE)
                        .Fail();
                    return NotFound(ErrorResult);
                }

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
    }
}

