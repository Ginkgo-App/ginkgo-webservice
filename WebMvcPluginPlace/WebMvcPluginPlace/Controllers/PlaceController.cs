using System;
using APICore.Helpers;
using APICore.Models;
using APICore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace WebMvcPluginPlace.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/" + PlaceVars.Version + "/places")]
    public class PlaceController : ControllerBase
    {
        private readonly IPlaceService _placeService;


        public PlaceController(IPlaceService placeService)
        {
            _placeService = placeService;
        }

        [AllowAnonymous]
        [HttpGet]
        public object GetAllPlaces([FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string type, [FromQuery] string keyword)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var errorCode = _placeService.TryGetAllPlaces(page, pageSize, type, keyword, out var places, out var pagination);

                    if (!errorCode)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }
                    
                    var jPlaces = new JArray();
                    
                    foreach (var p in places)
                    {
                        var isSuccess = _placeService.TryGetPlaceInfoById(p.Id, out var place);
                        if (!isSuccess) continue;
                        jPlaces.Add(place.ToJson());
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = jPlaces;
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
        [HttpGet("{Id}")]
        public object GetPlaceById(int id, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var errorCode = _placeService.TryGetPlaceInfoById(id, out var place);

                    if (!errorCode)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = new JArray {place.ToJson()};
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