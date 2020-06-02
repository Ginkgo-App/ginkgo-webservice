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

        #region Place

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

        [HttpGet("{id}")]
        public object GetPlaceById(int id)
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
                        || !CoreHelper.GetParameter(out var jsonTypeId, body, "TypeId", JTokenType.Integer,
                            ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonParentId, body, "ParentId", JTokenType.Integer,
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

                    var typeId = jsonTypeId != null
                        ? int.Parse(jsonTypeId.ToString())
                        : (int?) null;
                    var parentId = jsonParentId != null
                        ? int.Parse(jsonParentId.ToString())
                        : (int?) null;

                    if (typeId == null)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.InvalidParameter);
                        break;
                    }

                    var place = new Place((int) typeId, name, images, description);

                    if (!_placeService.TryAddPlace(place, parentId))
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
        public object Update(int id, [FromBody] object requestBody)
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
                            ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonTypeId, body, "TypeId", JTokenType.Integer,
                            ref responseModel, true))
                    {
                        break;
                    }

                    var images = jsonImages != null
                        ? JsonConvert.DeserializeObject<string[]>(jsonImages.ToString())
                        : null;

                    var typeId = jsonTypeId != null
                        ? int.Parse(jsonTypeId.ToString())
                        : (int?) null;

                    place.Update(jsonName?.ToString(), images, jsonDescription?.ToString(), typeId);

                    if (!_placeService.TryUpdatePlace(place))
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }
                    
                    if (!_placeService.TryGetPlaceInfoById(id, out var placeInfo) || placeInfo == null)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.PlaceNotFound);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = new JArray {placeInfo.ToJson()};
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

        #endregion
        
    }
}