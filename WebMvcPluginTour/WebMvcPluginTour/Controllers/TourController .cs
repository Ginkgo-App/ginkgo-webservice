using System;
using System.Collections.Generic;
using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using APICore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static APICore.Helpers.ErrorList;

namespace WebMvcPluginTour.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/" + TourVars.Version + "/tours")]
    public class TourController : ControllerBase
    {
        private readonly ITourInfoService _tourInfoService;
        private readonly IUserService _userService;
        private readonly IFriendService _friendService;
        private readonly ITourService _tourService;
        private readonly IServiceService _serviceService;

        public TourController(ITourInfoService tourInfoService, IUserService userService,
            IFriendService friendService, ITourService tourService, IServiceService serviceService)
        {
            _tourInfoService = tourInfoService;
            _userService = userService;
            _friendService = friendService;
            _tourService = tourService;
            _serviceService = serviceService;
        }

        [HttpGet("{tourId}")]
        public object GetTourDetail(int tourId)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    if (!_tourService.TryGetTour(tourId, out var tour))
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

        [HttpPost("{tourId}/join")]
        public object JoinTour(int tourId)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    // Get user id
                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    // Check user is exist
                    if (!_userService.TryGetUsers(userId, out var user))
                    {
                        responseModel.FromErrorCode(ErrorCode.UserNotFound);
                        break;
                    }

                    // Add tour to tour info
                    if (!_tourService.TryGetTour(tourId, out var tour))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    // Add join tour
                    if (!_tourService.TryJoinTour(tour, user))
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

        [HttpPost("{tourId}/accept/{userId}")]
        public object AcceptJoinTour(int userId, int tourId)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    // Get user id
                    var myUserId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    // Check user is exist
                    if (!_userService.TryGetUsers(userId, out var user))
                    {
                        responseModel.FromErrorCode(ErrorCode.UserNotFound);
                        break;
                    }

                    // Add tour to tour info
                    if (!_tourService.TryGetTour(tourId, out var tour))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    // User doest not have permission
                    if (myUserId != tour.CreateBy)
                    {
                        Response.StatusCode = 403;
                        responseModel.FromErrorCode(ErrorCode.UserDoesNotHavePermission);
                    }

                    // Add join tour
                    if (!_tourService.TryAcceptJoinTour(tour, user))
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

        [HttpPut("{tourId}")]
        public object UpdateTour(int tourInfoId, int tourId, [FromBody] object requestBody)
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
                            JTokenType.Date, ref responseModel, isNullable: true)
                        || !CoreHelper.GetParameter(out var jsonEndDate, body, "EndDay",
                            JTokenType.Date, ref responseModel, isNullable: true)
                        || !CoreHelper.GetParameter(out var jsonMaxMember, body, "MaxMember",
                            JTokenType.Integer, ref responseModel, isNullable: true)
                        || !CoreHelper.GetParameter(out var jsonTotalDay, body, "TotalDay",
                            JTokenType.Integer, ref responseModel, isNullable: true)
                        || !CoreHelper.GetParameter(out var jsonTotalNight, body, "TotalNight",
                            JTokenType.Integer, ref responseModel, isNullable: true)
                        || !CoreHelper.GetParameter(out var jsonPrice, body, "Price",
                            JTokenType.Float, ref responseModel, isNullable: true)
                        || !CoreHelper.GetParameter(out var jsonName, body, "Name",
                            JTokenType.String, ref responseModel, isNullable: true)
                        || !CoreHelper.GetParameter(out var jsonTimelines, body, "Timelines",
                            JTokenType.Array, ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonServices, body, "Services",
                            JTokenType.Array, ref responseModel, isNullable: true))
                    {
                        break;
                    }

                    if (_tourInfoService.TryGetTourInfoById(tourInfoId, out var tourInfo) != ErrorCode.Success ||
                        tourInfo == null)
                    {
                        throw new ExceptionWithMessage("Tour info not found.");
                    }

                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    // User dont have permission
                    if (tourInfo.CreateById != userId)
                    {
                        Response.StatusCode = 403;
                        break;
                    }

                    var name = jsonName?.ToString();
                    var isStartDayParse = DateTime.TryParse(jsonStartDate?.ToString(), out var startDate);
                    var isEndDayParse = DateTime.TryParse(jsonEndDate?.ToString(), out var endDate);
                    var isMaxMemberParse = int.TryParse(jsonMaxMember?.ToString(), out var maxMember);
                    var isTotalDayParsed = int.TryParse(jsonTotalDay?.ToString(), out var totalDay);
                    var isTotalNightParsed = int.TryParse(jsonTotalNight?.ToString(), out var totalNight);
                    var isPriceParsed = float.TryParse(jsonPrice?.ToString(), out var price);
                    var services = jsonServices != null
                        ? JsonConvert.DeserializeObject<string[]>(jsonServices.ToString())
                        : null;

                    var timelines = jsonTimelines?.ToObject<List<TimeLine>>();

                    if (!_tourService.TryGetTour(tourId, out var tour) || tour == null)
                    {
                        throw new ExceptionWithMessage("Tour not found.");
                    }

                    tour.Update(
                        name: name!,
                        timelines: timelines,
                        startDay: isStartDayParse ? startDate : (DateTime?) null,
                        endDay: isEndDayParse ? endDate : (DateTime?) null,
                        totalDay: isTotalDayParsed ? totalDay : (int?) null,
                        totalNight: isTotalNightParsed ? totalNight : (int?) null,
                        maxMember: isMaxMemberParse ? maxMember : (int?) null,
                        price: isPriceParsed ? price : (float?) null,
                        services: services
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
    }
}