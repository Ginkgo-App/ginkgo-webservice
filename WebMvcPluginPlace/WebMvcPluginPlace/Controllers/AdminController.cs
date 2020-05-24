using System;
using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using APICore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebMvcPluginPlace.Controllers
{
    [Authorize(Roles = RoleType.Admin)]
    [ApiController]
    [Route("api/" + PlaceVars.Version + "/admin/places")]
    public class AdminController
    {
        private readonly IPlaceService _placeService;


        public AdminController(IPlaceService placeService)
        {
            _placeService = placeService;
        }

        [HttpGet]
        public object GetAllPlaces([FromQuery] int page, [FromQuery] int pageSize)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var errorCode = _placeService.TryGetAllPlaces(page, pageSize, out var places, out var pagination);

                    if (!errorCode)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = JArray.FromObject(places);
                    responseModel.AdditionalProperties["Pagination"] = JObject.FromObject(pagination);
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpGet("{Id}")]
        public object GetPlaceById(int id, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var errorCode = _placeService.TryGetPlaceById(id, out var place);

                    if (!errorCode)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = new JArray{JToken.FromObject(place)};
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }
        
        [HttpPost]
        public object CreatePlace([FromBody] object requestBody)
        {
           var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var body = requestBody != null
                        ? JObject.Parse(requestBody.ToString() ?? "{}")
                        : null;

                    if (!CoreHelper.GetParameter(out var jsonName, body, "Name", JTokenType.String, 
                            ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonImages, body, "Images", JTokenType.Array,
                            ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonDescription, body, "Description", JTokenType.String,
                            ref responseModel, true))
                    {
                        break;
                    }

                    var name = jsonName?.ToString();
                    var description = jsonDescription?.ToString();
                    var images = jsonImages != null
                        ? JsonConvert.DeserializeObject<string[]>(jsonImages.ToString())
                        : null;

                    var place = new Place(name, images, description);

                    if (!_placeService.TryAddPlace(place))
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = new JArray {JToken.FromObject(place)};
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }
        
        [HttpPut("{Id}")]
        public object Update(int id,[FromBody] object requestBody)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    if (!_placeService.TryGetPlaceById(id, out var place) || place == null)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.PlaceNotFound);
                        break;
                    }
                    
                    var body = requestBody != null
                        ? JObject.Parse(requestBody.ToString() ?? "{}")
                        : null;

                    if (!CoreHelper.GetParameter(out var jsonName, body, "Name", JTokenType.String, 
                            ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonImages, body, "Images", JTokenType.Array,
                            ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonDescription, body, "Description", JTokenType.String,
                            ref responseModel, true))
                    {
                        break;
                    }
                    
                    var images = jsonImages != null
                        ? JsonConvert.DeserializeObject<string[]>(jsonImages.ToString())
                        : null;

                    place.Name = jsonName?.ToString() ?? place.Name;
                    place.Description = jsonDescription?.ToString() ?? place.Description;
                    place.Images = images ?? place.Images;

                    if (!_placeService.TryUpdatePlace(place))
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = new JArray {JToken.FromObject(place)};
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }
        
        [HttpDelete("{Id}")]
        public object DeletePlace(int id)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    if (!_placeService.TryRemovePlace(id))
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.PlaceNotFound);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = null;
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