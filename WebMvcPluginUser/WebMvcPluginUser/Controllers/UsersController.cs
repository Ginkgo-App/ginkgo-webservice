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
using Newtonsoft.Json.Linq;
using WebMvcPluginUser.Helpers;
using WebMvcPluginUser.Models;
using static APICore.Helpers.ErrorList;

namespace WebMvcPluginUser.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/" + UserVars.Version + "/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IFriendService _friendService;

        public UsersController(IUserService userService, IFriendService friendService)
        {
            _userService = userService;
            _friendService = friendService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public object Authenticate([FromBody] AuthenticateModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            JArray data = new JArray();
            string hashedPassword = UserHelper.HashPassword(model.Password);

            try
            {
                var errorCode = _userService.Authenticate(model.Email, hashedPassword, out User user);

                //data.Add(JObject.Parse(JsonConvert.SerializeObject(userHelper.WithoutPassword(user))));
                if (user == null || errorCode != ErrorCode.Success)
                {
                    responseModel.ErrorCode = (int) ErrorCode.UsernamePasswordIncorrect;
                    responseModel.Message = Description(responseModel.ErrorCode);
                }
                else
                {
                    data.Add(UserResponseJson(user));

                    responseModel.ErrorCode = (int) ErrorCode.Success;
                    responseModel.Message = Description(responseModel.ErrorCode);
                    responseModel.Data = data;
                }
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public object Register([FromBody] object requestBody)
        {
            var response = new ResponseModel();
            try
            {
                do
                {
                    var data = new JArray();

                    // Parse request body to json
                    var reqBody = requestBody != null
                        ? JObject.Parse(requestBody.ToString()!)
                        : null;

                    if (!CoreHelper.GetParameter(out var jsonName, reqBody, "Name", JTokenType.String, ref response)
                        || !CoreHelper.GetParameter(out var jsonPhoneNumber, reqBody, "PhoneNumber", JTokenType.String,
                            ref response)
                        || !CoreHelper.GetParameter(out var jsonEmail, reqBody, "Email", JTokenType.String,
                            ref response)
                        || !CoreHelper.GetParameter(out var jsonPassword, reqBody, "Password", JTokenType.String,
                            ref response)
                    )
                    {
                        break;
                    }

                    var name = jsonName.ToString();
                    var email = jsonEmail.ToString();
                    var phoneNumber = jsonPhoneNumber.ToString();
                    var hashedPassword = UserHelper.HashPassword(jsonPassword.ToString());

                    var statusCode = _userService.Register(name, email, phoneNumber, hashedPassword, out var user);

                    if (statusCode == ErrorCode.Success)
                    {
                        data.Add(UserResponseJson(user));
                    }

                    response.ErrorCode = (int) statusCode;
                    response.Message = Description(response.ErrorCode);
                    response.Data = data;
                } while (false);
            }
            catch (Exception ex)
            {
                response.FromException(ex);
            }

            return response;
        }

        [AllowAnonymous]
        [HttpPost("social-provider")]
        public object SocialProvider([FromBody] object requestBody)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                do
                {
                    var body = requestBody != null
                        ? JObject.Parse(requestBody.ToString()!)
                        : null;

                    if (!CoreHelper.GetParameter(out var jsonAccessToken, body, "AccessToken", JTokenType.String,
                            ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonType, body, "Type", JTokenType.String,
                            ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonEmail, body, "Email", JTokenType.String,
                            ref responseModel, isNullable: true))
                    {
                        break;
                    }

                    string accessToken = jsonAccessToken.ToString();
                    string type = jsonType.ToString();
                    string email = (jsonEmail ?? "").ToString();

                    if (type.Equals("facebook", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!_userService.TryGetFacebookInfo(accessToken, out AuthProvider authProvider))
                        {
                            break;
                        }

                        JArray data = new JArray();

                        ErrorCode errorCode = _userService.Authenticate(email, ref authProvider, out User user);

                        if (errorCode == ErrorCode.Success)
                        {
                            data.Add(UserResponseJson(user));
                            responseModel.ErrorCode = (int) ErrorCode.Success;
                            responseModel.Message = Description(responseModel.ErrorCode);
                            responseModel.Data = data;
                        }
                        else
                        {
                            responseModel.FromErrorCode(errorCode);
                        }
                    }
                    else
                    {
                        responseModel.FromErrorCode(ErrorCode.FeatureIsBeingImplemented);
                    }
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        //[Authorize(Roles = RoleType.Admin)]
        //[HttpGet]
        //public IActionResult GetAllUser(int userId, [FromQuery]int page, [FromQuery]int pageSize)
        //{
        //    ResponseModel responseModel = new ResponseModel();

        //    try
        //    {
        //        do
        //        {
        //            List<User> users = null;
        //            JArray data = new JArray();

        //            if (!_userService.TryGetUsers(page, pageSize, out users))
        //            {
        //                break;
        //            }

        //            if (users == null || users.Count == 0)
        //            {
        //                responseModel.ErrorCode = (int)ErrorList.ErrorCode.UserNotFound;
        //                responseModel.Message = ErrorList.Description(responseModel.ErrorCode);
        //                break;
        //            }

        //            foreach (var user in users)
        //            {
        //                data.Add(JObject.FromObject(user));
        //            }

        //            responseModel.ErrorCode = (int)ErrorList.ErrorCode.Success;
        //            responseModel.Message = ErrorList.Description(responseModel.ErrorCode);
        //            responseModel.Data = data;

        //        } while (false);
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.StatusCode = 501;
        //        responseModel.ErrorCode = 501;
        //        responseModel.Message = ex.Message;
        //    }

        //    return responseModel.ToJson();
        //}

        [HttpGet("{userId}")]
        public object GetUserById(int userId)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var data = new JArray();

                    if (!_userService.TryGetUsers(userId, out var user))
                    {
                        break;
                    }

                    if (user == null)
                    {
                        responseModel.ErrorCode = (int) ErrorCode.UserNotFound;
                        responseModel.Message = Description(responseModel.ErrorCode);
                        break;
                    }

                    data.Add(JObject.FromObject(user));
                    responseModel.ErrorCode = (int) ErrorCode.Success;
                    responseModel.Message = Description(responseModel.ErrorCode);
                    responseModel.Data = data;
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpDelete("{userId}")]
        public object DeleteUser(int userId)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    if (!_userService.TryRemoveUser(userId))
                    {
                        responseModel.ErrorCode = (int) ErrorCode.Fail;
                        responseModel.Message = "Remove user fail";
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

        [HttpPut("{userId}")]
        public object UpdateUser(int userId, [FromBody] object requestBody)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var body = requestBody != null
                        ? JObject.Parse(requestBody.ToString()!)
                        : null;

                    if (!CoreHelper.GetParameter(out var jsonPassword, body, "password", JTokenType.String,
                            ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonPhoneNumber, body, "phoneNumber", JTokenType.String,
                            ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonAddress, body, "address", JTokenType.String,
                            ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonAvatar, body, "avatar", JTokenType.String,
                            ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonSlogan, body, "slogan", JTokenType.String,
                            ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonBio, body, "bio", JTokenType.String, ref responseModel,
                            true)
                        || !CoreHelper.GetParameter(out var jsonJob, body, "job", JTokenType.String, ref responseModel,
                            true)
                        || !CoreHelper.GetParameter(out var jsonGender, body, "gender", JTokenType.String,
                            ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonBirthday, body, "birthday", JTokenType.Date,
                            ref responseModel, true))
                    {
                        break;
                    }

                    var password = jsonPassword?.ToString();
                    var phoneNumber = jsonPhoneNumber?.ToString();
                    var address = jsonAddress?.ToString();
                    var avatar = jsonAvatar?.ToString();
                    var slogan = jsonSlogan?.ToString();
                    var bio = jsonBio?.ToString();
                    var job = jsonJob?.ToString();
                    var gender = jsonGender?.ToString();
                    DateTime.TryParse(jsonBirthday?.ToString(), out _);

                    if (!_userService.TryGetUsers(userId, out var user))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    user.Password = password ?? user.Password;
                    user.PhoneNumber = phoneNumber ?? user.Password;
                    user.Address = address ?? user.Password;
                    user.Avatar = avatar ?? user.Password;
                    user.Slogan = slogan ?? user.Password;
                    user.Password = password ?? user.Password;
                    user.Bio = bio ?? user.Bio;
                    user.Job = job ?? user.Job;
                    user.Gender = gender ?? user.Gender;

                    var isSuccess = _userService.TryUpdateUser(user);

                    if (!isSuccess)
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray {JObject.FromObject(user)};
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpGet("me")]
        public object GetMyInfo()
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var identity = HttpContext.User.Identity as ClaimsIdentity;
                    if (identity == null)
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    var claims = identity.Claims;
                    int.TryParse(claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                        out var userId);
                    _userService.TryGetUsers(userId, out var user);

                    if (user == null)
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray
                    {
                        JObject.FromObject(user)
                    };
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpGet("me/tours")]
        public object GetMyTours()
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var identity = HttpContext.User.Identity as ClaimsIdentity;
                    if (identity == null)
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    var claims = identity.Claims;
                    int.TryParse(claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                        out var userId);
                    _userService.TryGetTours(userId, out List<TourInfo> tourInfos);

                    if (tourInfos == null)
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = JArray.FromObject(tourInfos);
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpGet("me/friends")]
        public object GetMyFriends([FromQuery] string type)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var identity = HttpContext.User.Identity as ClaimsIdentity;
                    if (identity == null)
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    var claims = identity.Claims;
                    int.TryParse(claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                        out var userId);

                    if (!_userService.TryGetFriends(userId, type, out var friends) || friends == null)
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = JArray.FromObject(friends);
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpPost("me/friends/{userToRequestId}")]
        public object AddFriend(int userToRequestId)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var identity = HttpContext.User.Identity as ClaimsIdentity;
                    if (identity == null)
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    var claims = identity.Claims;
                    int.TryParse(claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                        out var userId);

                    var errorCode = _friendService.TryAddFriend(userToRequestId, userId);
                    if (errorCode != ErrorCode.Success)
                    {
                        responseModel.FromErrorCode(errorCode);
                        break;
                    }

                    responseModel.FromErrorCode(errorCode);
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpPost("me/accept-friend/{userRequestId}")]
        public object AcceptFriendRequest(int userRequestId)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var identity = HttpContext.User.Identity as ClaimsIdentity;
                    if (identity == null)
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    var claims = identity.Claims;
                    int.TryParse(claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                        out var userId);

                    var errorCode = _friendService.TryAcceptFriend(userId, userRequestId);
                    if (errorCode != ErrorCode.Success)
                    {
                        responseModel.FromErrorCode(errorCode);
                        break;
                    }

                    responseModel.FromErrorCode(errorCode);
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpDelete("me/friends/{friendId}")]
        public object RemoveFriend(int friendId)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var identity = HttpContext.User.Identity as ClaimsIdentity;
                    if (identity == null)
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    var claims = identity.Claims;
                    int.TryParse(claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                        out var userId);

                    var errorCode = _friendService.TryRemoveFriend(userId, friendId);
                    if (errorCode != ErrorCode.Success)
                    {
                        responseModel.FromErrorCode(errorCode);
                        break;
                    }

                    responseModel.FromErrorCode(errorCode);
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        private static JObject UserResponseJson(User user)
        {
            var jObject = new JObject {{"Id", user.Id}, {"Token", user.Token}};

            return jObject;
        }
        //[HttpGet("{userId}")]
        //public IActionResult Example(string userId)
        //{
        //    ResponseModel responseModel = new ResponseModel();

        //    try
        //    {
        //        do
        //        {
        //        } while (false);
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.StatusCode = 501;
        //        responseModel.ErrorCode = 501;
        //        responseModel.Message = ex.Message;
        //    }

        //    return responseModel.ToJson();
        //}
    }
}