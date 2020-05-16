using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using WebMvcPluginTour.Services;
using static APICore.Helpers.ErrorList;

namespace WebMvcPluginTour.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/" + TourVars.Version + "/tour-infos")]
    public class TourInfoController : ControllerBase
    {
        private ITourInfoService _tourInfoService;

        public TourInfoController(ITourInfoService tourInfoService)
        {
            _tourInfoService = tourInfoService;
        }

        [HttpGet("{id}/tours")]
        [AllowAnonymous]
        public object GetAllTours(string id, [FromQuery]int page, [FromQuery]int pageSize)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                do
                {
                    List<JObject> tourFullInfos = null;
                    Pagination pagination = null;
                    JArray data = new JArray();

                    //if (!_tourInfoService.TryGetTours(page, pageSize, out tourInfos, out pagination))
                    //{
                    //    break;
                    //}

                    //if (users == null || users.Count == 0 || pagination == null)
                    //{
                    //    responseModel.ErrorCode = (int)ErrorList.ErrorCode.UserNotFound;
                    //    responseModel.Message = ErrorList.Description(responseModel.ErrorCode);
                    //    break;
                    //}
                    


                    // Add data to Respone
                    foreach (var tourFullInfo in tourFullInfos)
                    {
                        data.Add(JObject.FromObject(tourFullInfo));
                    }

                    responseModel.ErrorCode = (int)ErrorList.ErrorCode.Success;
                    responseModel.Message = ErrorList.Description(responseModel.ErrorCode);
                    responseModel.Data = data;
                    responseModel.AdditionalProperties["Pagination"] = JObject.FromObject(pagination);

                } while (false);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 501;
                responseModel.ErrorCode = 501;
                responseModel.Message = ex.Message;
            }

            return responseModel.ToJson();
        }

        [HttpGet]
        [AllowAnonymous]
        public object GetAllTourInfos([FromQuery]int page, [FromQuery]int pageSize)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                do
                {
                    List<TourInfo> tourInfos = null;
                    Pagination pagination = null;
                    JArray data = new JArray();
                        
                    if (_tourInfoService.TryGetTours(page, pageSize, out tourInfos, out pagination) != ErrorCode.Success)
                    {
                        break;
                    }

                    if (tourInfos == null || tourInfos.Count == 0 || pagination == null)
                    {
                        responseModel.ErrorCode = (int)ErrorList.ErrorCode.UserNotFound;
                        responseModel.Message = ErrorList.Description(responseModel.ErrorCode);
                        break;
                    }


                    // Add data to Respone
                    foreach (var tourInfo in tourInfos)
                    {
                        data.Add(AddTourFullInfo(tourInfo));
                    }

                    responseModel.ErrorCode = (int)ErrorList.ErrorCode.Success;
                    responseModel.Message = ErrorList.Description(responseModel.ErrorCode);
                    responseModel.Data = data;
                    responseModel.AdditionalProperties["Pagination"] = JObject.FromObject(pagination);

                } while (false);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 501;
                responseModel.ErrorCode = 501;
                responseModel.Message = ex.Message;
            }

            return responseModel.ToJson();
        }

        [HttpGet("{id}")]
        public object GetTourInfo(int id)
        {
            ResponseModel responseModel = new ResponseModel();
            ErrorCode errorCode;

            try
            {
                do
                {
                    errorCode = _tourInfoService.TryGetTourInfoById(id, out TourInfo tourInfo);
                    if (errorCode != ErrorCode.Success)
                    {
                        responseModel.FromErrorCode(errorCode);
                        break;
                    }

                    if (tourInfo == null)
                    {
                        responseModel.FromErrorCode(ErrorCode.TourNotFound);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray { AddTourFullInfo(tourInfo) };
                } while (false);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 501;
                responseModel.ErrorCode = 501;
                responseModel.Message = ex.Message;
            }

            return responseModel.ToJson();
        }

        [HttpPost]
        public object CreateTourInfo([FromBody]object requestBody)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                do
                {
                    var identity = HttpContext.User.Identity as ClaimsIdentity;
                    if (identity == null)
                    {
                        break;
                    }

                    IEnumerable<Claim> claims = identity.Claims;
                    
                    int userId = int.Parse(
                        claims.Where(x => x.Type == ClaimTypes.NameIdentifier)
                        .FirstOrDefault().Value);

                    JObject body = requestBody != null
                        ? JObject.Parse(requestBody.ToString())
                        : null;

                    JArray data = new JArray();

                    if (!CoreHelper.GetParameter(out JToken jsonDestinatePlaceId, body, "DestinatePlaceId", JTokenType.Integer, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonStartPlaceId, body, "StartPlaceId", JTokenType.Integer, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonImages, body, "Images", JTokenType.Array, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonName, body, "Name", JTokenType.String, ref responseModel, true))
                    {
                        break;
                    }

                    bool isDestinatePlaceId = int.TryParse(jsonDestinatePlaceId?.ToString(), out int destinatePlaceId);
                    bool isStartPlaceId = int.TryParse(jsonStartPlaceId?.ToString(), out int startPlaceId);
                    string name = jsonName?.ToString();
                    string[] images = jsonImages != null
                        ? JsonConvert.DeserializeObject<string[]>(jsonImages.ToString())
                        : null;

                    var tourInfo = new TourInfo(
                        createById: userId,
                        name: name,
                        images: images,
                        startPlaceId: startPlaceId,
                        destinatePlaceId: destinatePlaceId
                        );

                    if (_tourInfoService.TryAddTourInfo(tourInfo))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray { AddTourFullInfo(tourInfo) };

                } while (false);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 501;
                responseModel.ErrorCode = 501;
                responseModel.Message = ex.Message;
            }

            return responseModel.ToJson();
        }

        [HttpPut("{id}")]
        public object UpdateTourInfo(int id, [FromBody]object requestBody)
        {
            ResponseModel responseModel = new ResponseModel();
            ErrorCode errorCode;

            try
            {
                do
                {
                    errorCode = _tourInfoService.TryGetTourInfoById(id, out TourInfo tourInfo);
                    if (errorCode != ErrorCode.Success)
                    {
                        responseModel.FromErrorCode(errorCode);
                        break;
                    }

                    if (tourInfo == null)
                    {
                        responseModel.FromErrorCode(ErrorCode.TourNotFound);
                        break;
                    }

                    JObject body = requestBody != null
                        ? JObject.Parse(requestBody.ToString())
                        : null;

                    JArray data = new JArray();

                    if (!CoreHelper.GetParameter(out JToken jsonDestinatePlaceId, body, "DestinatePlaceId", JTokenType.Integer, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonStartPlaceId, body, "StartPlaceId", JTokenType.Integer, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonImages, body, "Images", JTokenType.Array, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonName, body, "Name", JTokenType.String, ref responseModel, true))
                    {
                        break;
                    }

                    bool isDestinatePlaceId = int.TryParse(jsonDestinatePlaceId?.ToString(), out int destinatePlaceId);
                    bool isStartPlaceId = int.TryParse(jsonStartPlaceId?.ToString(), out int startPlaceId);
                    string name = jsonName?.ToString();
                    string[] images = jsonImages != null
                        ? JsonConvert.DeserializeObject<string[]>(jsonImages.ToString())
                        : null;

                    tourInfo.Images = images ?? tourInfo.Images;
                    tourInfo.Name = name ?? tourInfo.Name;
                    tourInfo.StartPlaceId = isStartPlaceId ? startPlaceId : tourInfo.StartPlaceId;
                    tourInfo.DestinatePlaceId = isDestinatePlaceId ? destinatePlaceId : tourInfo.StartPlaceId;

                    if (!_tourInfoService.TryUpdateTourInfo(tourInfo))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray { AddTourFullInfo(tourInfo) };

                } while (false);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 501;
                responseModel.ErrorCode = 501;
                responseModel.Message = ex.Message;
            }

            return responseModel.ToJson();
        }

        [HttpDelete("{id}")]
        public object DeleteTourInfo(int id)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                do
                {
                    if (!_tourInfoService.TryRemoveTourInfo(id))
                    {
                        responseModel.ErrorCode = (int)ErrorCode.Fail;
                        responseModel.Message = "Remove tour fail";
                        break;
                    }
                    responseModel.ErrorCode = (int)ErrorCode.Success;
                    responseModel.Message = ErrorList.Description(responseModel.ErrorCode);
                } while (false);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 501;
                responseModel.ErrorCode = 501;
                responseModel.Message = ex.Message;
            }

            return responseModel.ToJson();
        }

        private JObject AddTourFullInfo(TourInfo tourInfo)
        {
            JObject result = null;
            do
            {
                if (tourInfo == null)
                {
                    break;
                }

                result = JObject.FromObject(tourInfo);

                _tourInfoService.TryGetPlaceById(tourInfo.StartPlaceId, out Place starPlace);
                _tourInfoService.TryGetPlaceById(tourInfo.DestinatePlaceId, out Place destinationPlace);

                result.Remove("StartPlaceId");
                result.Remove("DestinatePlaceId");
                result.Remove("CreateById");

                result.Add("StartPlace", starPlace == null ? null : JObject.FromObject(starPlace));
                result.Add("DestinatePlaceId", destinationPlace == null ? null : JObject.FromObject(destinationPlace));
            } while (false);
            return result;
        }
    }
}
