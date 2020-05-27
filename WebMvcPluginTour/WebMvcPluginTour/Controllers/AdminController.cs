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
using static APICore.Helpers.ErrorList;

namespace WebMvcPluginTour.Controllers
{
    [Authorize(Roles = RoleType.Admin)]
    [ApiController]
    [Route("api/" + TourVars.Version + "/admin/tour-infos")]
    public class AdminController : ControllerBase
    {
        private readonly ITourInfoService _tourInfoService;
        private readonly IUserService _userService;
        private readonly IFriendService _friendService;
        private readonly ITourService _tourService;

        public AdminController(ITourInfoService tourInfoService, IUserService userService, IFriendService friendService,
            ITourService tourService)
        {
            _tourInfoService = tourInfoService;
            _userService = userService;
            _friendService = friendService;
            _tourService = tourService;
        }

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

                    if (!_userService.TryGetUsers(tourInfo.CreateById, out var host))
                    {
                        responseModel.FromErrorCode(ErrorCode.UserNotFound);
                        break;
                    }

                    // Cast to ClaimsIdentity.
                    var identity = HttpContext.User.Identity as ClaimsIdentity;

                    // Gets list of claims.
                    IEnumerable<Claim> claim = identity?.Claims;

                    // Gets userId from claims. Generally it's an email address.
                    var userIdString = claim
                        ?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)
                        ?.Value;

                    if (string.IsNullOrEmpty(userIdString))
                    {
                        responseModel.FromErrorCode(ErrorCode.UserNotFound);
                        break;
                    }

                    // Convert userId to int
                    var userId = int.Parse(userIdString);

                    var eIsFriend = string.IsNullOrEmpty(userIdString)
                        ? 0
                        : _friendService.CalculateIsFriend(userId, host.Id);

                    // Add data to Response
                    foreach (var tour in tours)
                    {
                        _ = _tourService.TryGetTotalMember(tour.Id, out var totalMember);

                        data.Add(tour.ToSimpleJson(host, eIsFriend, totalMember, null!));
                    }

                    responseModel.ErrorCode = (int) ErrorCode.Success;
                    responseModel.Message = Description(responseModel.ErrorCode);
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

        [HttpGet("{id}/tours/{tourId}")]
        public object GetTourInfo(int id, int tourId)
        {
            var responseModel = new ResponseModel();

            var data = new JArray();
            try
            {
                do
                {
                    if (_tourInfoService.TryGetTourInfoById(id, out var tourInfo) != ErrorCode.Success ||
                        tourInfo == null)
                    {
                        throw new ExceptionWithMessage("Tour info not found.");
                    }

                    if (!_tourService.TryGetTour(id, out var tour) || tour == null)
                    {
                        throw new ExceptionWithMessage("Tour not found.");
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray {JObject.FromObject(tour)};
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpPost("{id}/tours")]
        public object AddNewTour(int id, [FromBody] object requestBody)
        {
            var data = new JArray();
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var body = requestBody != null
                        ? JObject.Parse(requestBody.ToString() ?? "{}")
                        : null;

                    if (!CoreHelper.GetParameter(out var jsonStartDate, body, "StartDay",
                            JTokenType.Date, ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonEndDate, body, "EndDay",
                            JTokenType.Date, ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonMaxMember, body, "MaxMember",
                            JTokenType.Integer, ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonName, body, "Name",
                            JTokenType.String, ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonServiceIds, body, "ServiceIds",
                            JTokenType.Array, ref responseModel))
                    {
                        break;
                    }

                    if (_tourInfoService.TryGetTourInfoById(id, out var tourInfo) != ErrorCode.Success ||
                        tourInfo == null)
                    {
                        throw new ExceptionWithMessage("Tour info not found.");
                    }

                    var name = jsonName?.ToString();
                    _ = DateTime.TryParse(jsonStartDate?.ToString(), out var startDate);
                    _ = DateTime.TryParse(jsonEndDate?.ToString(), out var endDate);
                    _ = int.TryParse(jsonMaxMember?.ToString(), out var maxMember);
                    var serviceIds = jsonServiceIds != null
                        ? JsonConvert.DeserializeObject<string[]>(jsonServiceIds.ToString())
                        : null;

                    var tour = new Tour(
                        name: name,
                        startDay: startDate,
                        endDay: endDate,
                        maxMember: maxMember,
                        tourInfoId: id
                    );

                    if (!_tourService.TryAddTour(tour))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray {JObject.FromObject(tour)};
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpPut("{id}/tours/{tourId}")]
        public object UpdateTour(int id, int tourId, [FromBody] object requestBody)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var body = requestBody != null
                        ? JObject.Parse(requestBody.ToString() ?? "{}")
                        : null;

                    if (!CoreHelper.GetParameter(out var jsonStartDate, body, "StartDay",
                            JTokenType.Date, ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonEndDate, body, "EndDay",
                            JTokenType.Date, ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonMaxMember, body, "MaxMember",
                            JTokenType.Integer, ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonName, body, "Name",
                            JTokenType.String, ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonServiceIds, body, "ServiceIds",
                            JTokenType.Array, ref responseModel))
                    {
                        break;
                    }

                    if (_tourInfoService.TryGetTourInfoById(id, out var tourInfo) != ErrorCode.Success ||
                        tourInfo == null)
                    {
                        throw new ExceptionWithMessage("Tour info not found.");
                    }

                    var name = jsonName?.ToString();
                    var isStartDayParse = DateTime.TryParse(jsonStartDate?.ToString(), out var startDate);
                    var isEndDayParse = DateTime.TryParse(jsonEndDate?.ToString(), out var endDate);
                    var isMaxMemberParse = int.TryParse(jsonMaxMember?.ToString(), out var maxMember);
                    var serviceIds = jsonServiceIds != null
                        ? JsonConvert.DeserializeObject<string[]>(jsonServiceIds.ToString())
                        : null;

                    if (!_tourService.TryGetTour(tourId, out var tour) || tour == null)
                    {
                        throw new ExceptionWithMessage("Tour not found.");
                    }

                    tour.Update(
                        name: name!,
                        startDay: isStartDayParse ? startDate : (DateTime?) null,
                        endDay: isEndDayParse ? endDate : (DateTime?) null,
                        maxMember: isMaxMemberParse ? maxMember : (int?) null
                    );

                    if (!_tourService.TryUpdateTour(tour))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray {JObject.FromObject(tour)};
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpDelete("{id}/tours/{tourId}")]
        public object DeleteTour(int id, int tourId)
        {
            var responseModel = new ResponseModel();

            var data = new JArray();
            try
            {
                do
                {
                    if (_tourInfoService.TryGetTourInfoById(id, out var tourInfo) != ErrorCode.Success ||
                        tourInfo == null)
                    {
                        throw new ExceptionWithMessage("Tour info not found.");
                    }

                    if (!_tourService.TryDeleteTour(tourId))
                    {
                        throw new ExceptionWithMessage("Tour not found.");
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray();
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpGet]
        public object GetAllTourInfos([FromQuery] int page, [FromQuery] int pageSize)
        {
            var responseModel = new ResponseModel();
            try
            {
                do
                {
                    var data = new JArray();

                    if (_tourInfoService.TryGetTourInfos(page, pageSize, out var tourInfos, out var pagination) !=
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
                responseModel.FromException(ex);
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
                responseModel.FromException(ex);
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

                    var isDestinationParsed =
                        int.TryParse(jsonDestinationPlaceId?.ToString(), out var destinationPlaceId);
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
                        name: name!,
                        images: images!,
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
                responseModel.FromException(ex);
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
                    var images = jsonImages != null
                        ? JsonConvert.DeserializeObject<string[]>(jsonImages.ToString())
                        : null;

                    tourInfo.Update(
                        createById: null,
                        name: jsonName?.ToString(),
                        images: images!,
                        startPlaceId: isStartPlaceId ? startPlaceId : (int?) null,
                        destinatePlaceId: isDestinationPlaceId ? destinationPlaceId : (int?) null
                    );

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
                responseModel.FromException(ex);
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
                    responseModel.Message = Description(responseModel.ErrorCode);
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
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