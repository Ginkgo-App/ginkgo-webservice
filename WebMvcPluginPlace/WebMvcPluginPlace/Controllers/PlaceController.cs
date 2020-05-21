using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using APICore.Services;
using APICore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebMvcPluginPlace.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/" + PlaceVars.Version + "/tour-infos")]
    public class TourInfoController : ControllerBase
    {
        private readonly IPlaceService _placeService;


        public TourInfoController(IPlaceService placeService)
        {
            _placeService = placeService;
        }

        [AllowAnonymous]
        [HttpGet]
        public object GetAllPlaces([FromQuery] int page, [FromQuery] int pageSize)
        {
            var responseModel = new ResponseModel();

            var data = new JArray();
            try
            {
                do
                {
                    var errorCode = _placeService.TryGetAllPlaces(page, pageSize, out var places, out var pagination);

                    if (errorCode)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = data;
                    responseModel.AdditionalProperties["Pagination"] = JObject.FromObject(pagination);
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public object GetAllPlacesById(int id, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var responseModel = new ResponseModel();

            var data = new JArray();
            try
            {
                do
                {
                    var errorCode = _placeService.TryGetPlaceById(id, out var places);

                    if (errorCode)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = data;
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }
    }
}