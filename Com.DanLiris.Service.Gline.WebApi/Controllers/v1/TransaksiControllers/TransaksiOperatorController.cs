﻿using AutoMapper;
using Com.DanLiris.Service.Gline.Lib.Interfaces;
using Com.DanLiris.Service.Gline.Lib.Models.TransaksiModel;
using Com.DanLiris.Service.Gline.Lib.Services;
using Com.DanLiris.Service.Gline.Lib.ViewModels.TransaksiViewModel;
using Com.DanLiris.Service.Gline.WebApi.Helpers;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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

        private readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;
        private readonly IMapper mapper;
        private readonly ITransaksiOperatorFacade facade;

        public TransaksiOperatorController(IServiceProvider serviceProvider, ITransaksiOperatorFacade facade, IMapper mapper)
        {
            this.serviceProvider = serviceProvider;
            this.mapper = mapper;
            this.facade = facade;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
        }

        [HttpGet]
        public IActionResult Get(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}")
        {
            try
            {
                var Data = facade.Read(page, size, order, keyword, filter);
                var newData = mapper.Map<List<TransaksiOperatorViewModel>>(Data.Item1);

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
        public async Task<IActionResult> Post([FromBody] TransaksiOperatorCreateModel viewModel)
        {
            identityService.Token = Request.Headers["Authorization"].First().Replace("Bearer ", "");
            identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

            IValidateService validateService = (IValidateService)serviceProvider.GetService(typeof(IValidateService));

            try
            {
                viewModel.Id = Guid.NewGuid().ToString();
                validateService.Validate(viewModel);

                TransaksiOperator model = mapper.Map<TransaksiOperator>(viewModel);

                int result = await facade.Create(model, identityService.Username);

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
