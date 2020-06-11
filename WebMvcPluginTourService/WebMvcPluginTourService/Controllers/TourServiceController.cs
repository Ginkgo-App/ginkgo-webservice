using System;
using System.Linq;
using System.Security.Claims;
using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using APICore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using static APICore.Helpers.ErrorList;

namespace WebMvcPluginTourService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/" + ServiceVars.Version + "/services")]
    public class TourServiceController : ControllerBase
    {
        private readonly ITourInfoService _tourInfoService;
        private readonly IUserService _userService;
        private readonly IFriendService _friendService;
        private readonly ITourService _tourService;
        private readonly IServiceService _serviceService;

        public TourServiceController(ITourInfoService tourInfoService, IUserService userService,
            IFriendService friendService, ITourService tourService, IServiceService serviceService)
        {
            _tourInfoService = tourInfoService;
            _userService = userService;
            _friendService = friendService;
            _tourService = tourService;
            _serviceService = serviceService;
        }

        [HttpGet]
        public object GetAllServices(int id, [FromQuery] int page, [FromQuery] int pageSize)
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
                    var claim = identity?.Claims;

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

                    var eIsFriend = _friendService.CalculateIsFriend(userId, host.Id);

                    // Add data to Response
                    foreach (var tour in tours)
                    {
                        _ = _tourService.TryGetTotalMember(tour.Id, out var totalMember);

                        data.Add(tour.ToSimpleJson(host, eIsFriend, totalMember, tourInfo));
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
        public object GetServiceById(int id)
        {
            var responseModel = new ResponseModel();
            try
            {
                do
                {
                    if (_serviceService.TryGetServiceById(id, out var service))
                    {
                        break;
                    }

                    if (service == null)
                    {
                        responseModel.FromErrorCode(ErrorCode.ServiceNotFound);
                        break;
                    }

                    responseModel.ErrorCode = (int) ErrorCode.Success;
                    responseModel.Message = Description(responseModel.ErrorCode);
                    responseModel.Data = new JArray {service.ToJson()};
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpPost]
        public object CreateTourService([FromBody] object requestBody)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var body = requestBody != null
                        ? JObject.Parse(requestBody.ToString() ?? "{}")
                        : null;

                    if (!CoreHelper.GetParameter(out var jsonName, body, "name",
                            JTokenType.String, ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonImage, body, "image",
                            JTokenType.String, ref responseModel, true))
                    {
                        break;
                    }

                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    var tourService = new Service(
                        name: jsonName.ToString(),
                        image: jsonImage?.ToString(),
                        deletedAt: null
                    );

                    if (!_serviceService.TryAddService(tourService, userId))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray {tourService.ToJson()};
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpDelete("{id}")]
        public object DeleteTourService(int id)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    if (!_serviceService.TryDeleteService(id))
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

        [HttpPut("{id}")]
        public object UpdateTourService(int id, [FromBody] object requestBody)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var body = requestBody != null
                        ? JObject.Parse(requestBody.ToString() ?? "{}")
                        : null;

                    if (!CoreHelper.GetParameter(out var jsonName, body, "Name",
                            JTokenType.String, ref responseModel, isNullable: true)
                        || !CoreHelper.GetParameter(out var jsonImages, body, "Image",
                            JTokenType.String, ref responseModel, isNullable: true))
                    {
                        break;
                    }

                    if (!_serviceService.TryGetServiceById(id, out var service) || service == null)
                    {
                        throw new ExceptionWithMessage("Service not found");
                    }

                    service.Update(jsonName?.ToString()!, jsonImages?.ToString()!);

                    // Add tour to tour info
                    if (!_serviceService.TryUpdateService(service))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray {service.ToJson()};
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