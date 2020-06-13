using System;
using APICore.Helpers;
using APICore.Models;
using APICore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    }
}