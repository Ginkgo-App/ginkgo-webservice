using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using APICore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Security.Claims;
using static APICore.Helpers.ErrorList;

namespace WebMvcPluginTour.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/" + TourVars.Version + "/tour-infos")]
    public class TourInfoController : ControllerBase
    {
        private readonly ITourInfoService _tourInfoService;
        private readonly IUserService _userService;

        public TourInfoController(ITourInfoService tourInfoService, IUserService userService)
        {
            _tourInfoService = tourInfoService;
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpGet("{id}/tours")]
        public object GetAllTours(int id, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var responseModel = new ResponseModel();

            var data = new JArray();
            try
            {
                do
                {
                    var errorCode = _tourInfoService.TryGetTours(id, page, pageSize, out var tours, out var pagination);

                    if (errorCode != ErrorCode.Success)
                    {
                        responseModel.FromErrorCode(errorCode);
                        break;
                    }

                    errorCode = _tourInfoService.TryGetTourInfoById(id, out var tourInfo);

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

                    if (_userService.TryGetUsers(tourInfo.CreateById, out var host))
                    {
                        responseModel.FromErrorCode(ErrorCode.UserNotFound);
                        break;
                    }

                    // Add data to Response
                    foreach (var tour in tours)
                    {
                        data.Add(tour.ToSimpleJson(host, 0, 0, null));
                        //data.Add(AddTourFullInfo(tourInfo));
                    }

                    responseModel.ErrorCode = (int) ErrorCode.Success;
                    responseModel.Message = Description(responseModel.ErrorCode);
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
        public object GetAllTourInfos([FromQuery] int page, [FromQuery] int pageSize)
        {
            var responseModel = new ResponseModel();
            try
            {
                do
                {
                    var data = new JArray();

                    if ( _tourInfoService.TryGetTourInfos(page, pageSize, out var tourInfos, out var pagination) !=
                        ErrorCode.Success)
                    {
                        break;
                    }

                    if (tourInfos == null || tourInfos.Count == 0 || pagination == null)
                    {
                        responseModel.ErrorCode = (int) ErrorCode.UserNotFound;
                        responseModel.Message = Description(responseModel.ErrorCode);
                        break;
                    }

                    // Add data to Response
                    foreach (var tourInfo in tourInfos)
                    {
                        data.Add(AddTourFullInfo(tourInfo));
                    }

                    responseModel.ErrorCode = (int) ErrorCode.Success;
                    responseModel.Message = Description(responseModel.ErrorCode);
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

            try
            {
                do
                {
                    var errorCode = _tourInfoService.TryGetTourInfoById(id, out TourInfo tourInfo);
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
                    responseModel.Data = new JArray {AddTourFullInfo(tourInfo)};
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
        public object CreateTourInfo([FromBody] object requestBody)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var identity = HttpContext.User.Identity as ClaimsIdentity;
                    if (identity == null)
                    {
                        break;
                    }

                    var claims = identity.Claims;

                    var userId = int.Parse(
                        claims
                            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)
                            ?.Value ?? "0");

                    var body = requestBody != null
                        ? JObject.Parse(requestBody.ToString() ?? "{}")
                        : null;

                    if (!CoreHelper.GetParameter(out var jsonDestinationPlaceId, body, "DestinationPlaceId",
                            JTokenType.Integer, ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonStartPlaceId, body, "StartPlaceId",
                            JTokenType.Integer, ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonImages, body, "Images", JTokenType.Array,
                            ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonName, body, "Name", JTokenType.String,
                            ref responseModel, true))
                    {
                        break;
                    }

                    var isDestinationParsed = int.TryParse(jsonDestinationPlaceId?.ToString(), out var destinationPlaceId);
                    var isStartPlaced = int.TryParse(jsonStartPlaceId?.ToString(), out var startPlaceId);

                    if (!isDestinationParsed || !isStartPlaced)
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    var name = jsonName?.ToString();
                    var images = jsonImages != null
                        ? JsonConvert.DeserializeObject<string[]>(jsonImages.ToString())
                        : null;

                    var tourInfo = new TourInfo(
                        createById: userId,
                        name: name,
                        images: images,
                        startPlaceId: startPlaceId,
                        destinatePlaceId: destinationPlaceId
                    );

                    if (_tourInfoService.TryAddTourInfo(tourInfo))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray {AddTourFullInfo(tourInfo)};
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
        public object UpdateTourInfo(int id, [FromBody] object requestBody)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var errorCode = _tourInfoService.TryGetTourInfoById(id, out var tourInfo);
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

                    var body = requestBody != null
                        ? JObject.Parse(requestBody.ToString() ?? "{}")
                        : null;

                    if (!CoreHelper.GetParameter(out var jsonDestinationPlaceId, body, "DestinationPlaceId",
                            JTokenType.Integer, ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonStartPlaceId, body, "StartPlaceId",
                            JTokenType.Integer, ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonImages, body, "Images", JTokenType.Array,
                            ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonName, body, "Name", JTokenType.String,
                            ref responseModel, true))
                    {
                        break;
                    }

                    var isDestinationPlaceId =
                        int.TryParse(jsonDestinationPlaceId?.ToString(), out var destinationPlaceId);
                    var isStartPlaceId = int.TryParse(jsonStartPlaceId?.ToString(), out var startPlaceId);
                    var name = jsonName?.ToString();
                    var images = jsonImages != null
                        ? JsonConvert.DeserializeObject<string[]>(jsonImages.ToString())
                        : null;

                    tourInfo.Images = images ?? tourInfo.Images;
                    tourInfo.Name = name ?? tourInfo.Name;
                    tourInfo.StartPlaceId = isStartPlaceId ? startPlaceId : tourInfo.StartPlaceId;
                    tourInfo.DestinatePlaceId = isDestinationPlaceId ? destinationPlaceId : tourInfo.DestinatePlaceId;

                    if (!_tourInfoService.TryUpdateTourInfo(tourInfo))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray {AddTourFullInfo(tourInfo)};
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
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    if (!_tourInfoService.TryRemoveTourInfo(id))
                    {
                        responseModel.ErrorCode = (int) ErrorCode.Fail;
                        responseModel.Message = "Remove tour fail";
                        break;
                    }

                    responseModel.ErrorCode = (int) ErrorCode.Success;
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

                _tourInfoService.TryGetPlaceById(tourInfo.StartPlaceId, out var starPlace);
                _tourInfoService.TryGetPlaceById(tourInfo.DestinatePlaceId, out var destinationPlace);

                result = tourInfo.ToJson(starPlace, destinationPlace);
            } while (false);

            return result;
        }
    }
}