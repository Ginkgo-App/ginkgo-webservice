using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using WebMvcPluginUser.Entities;
using WebMvcPluginUser.Helpers;
using WebMvcPluginUser.Models;
using WebMvcPluginUser.Services;
using static APICore.Helpers.ErrorList;

namespace WebMvcPluginUser.Controllers
{
    [Authorize(Roles = RoleType.Admin)]
    [ApiController]
    [Route("api/" + UserVars.Version + "/admin")]
    public class AdminController : ControllerBase
    {
        private IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult GetAllUser(int userId, [FromQuery]int page, [FromQuery]int pageSize)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                do
                {
                    List<User> users = null;
                    JArray data = new JArray();

                    if (!_userService.TryGetUsers(page, pageSize, out users))
                    {
                        break;
                    }

                    if (users == null || users.Count == 0)
                    {
                        responseModel.ErrorCode = (int)ErrorList.ErrorCode.UserNotFound;
                        responseModel.Message = ErrorList.Description(responseModel.ErrorCode);
                        break;
                    }

                    foreach (var user in users)
                    {
                        data.Add(JObject.FromObject(user));
                    }

                    responseModel.ErrorCode = (int)ErrorList.ErrorCode.Success;
                    responseModel.Message = ErrorList.Description(responseModel.ErrorCode);
                    responseModel.Data = data;
                    responseModel.AdditionalProperties["Pagination"] = new JObject {
                        new JProperty("test", 123)
                    };

                } while (false);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 501;
                responseModel.ErrorCode = 501;
                responseModel.Message = ex.Message;
            }

            return Ok(responseModel.ToString());
        }

        [HttpGet("users/{userId}")]
        public IActionResult GetUserById(int userId)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                do
                {
                    User user = null;
                    JArray data = new JArray();

                    if (!_userService.TryGetUsers(userId, out user))
                    {
                        break;
                    }

                    if (user == null)
                    {
                        responseModel.ErrorCode = (int)ErrorList.ErrorCode.UserNotFound;
                        responseModel.Message = ErrorList.Description(responseModel.ErrorCode);
                        break;
                    }

                    data.Add(JObject.FromObject(user));
                    responseModel.ErrorCode = (int)ErrorList.ErrorCode.Success;
                    responseModel.Message = ErrorList.Description(responseModel.ErrorCode);
                    responseModel.Data = data;

                } while (false);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 501;
                responseModel.ErrorCode = 501;
                responseModel.Message = ex.Message;
            }

            return Ok(responseModel.ToString());
        }

        [HttpDelete("users/{userId}")]
        public IActionResult DeleteUser(int userId)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                do
                {
                    if (!_userService.TryRemoveUser(userId))
                    {
                        responseModel.ErrorCode = (int)ErrorCode.Fail;
                        responseModel.Message = "Remove user fail";
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

            return Ok(responseModel.ToString());
        }

        [HttpPost("users")]
        public IActionResult AddUser([FromBody]object requestBody)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                do
                {
                    JObject body = requestBody != null
                        ? JObject.Parse(requestBody.ToString())
                        : null;

                    JArray data = new JArray();

                    if (!CoreHelper.GetParameter(out JToken jsonPassword, body, "password", JTokenType.String, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonPhonenumber, body, "phonenumber", JTokenType.String, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonAddress, body, "address", JTokenType.String, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonAvatar, body, "avatar", JTokenType.String, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonSlogan, body, "slogan", JTokenType.String, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonBio, body, "bio", JTokenType.String, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonJob, body, "job", JTokenType.String, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonGender, body, "gender", JTokenType.String, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonBirthday, body, "birthday", JTokenType.Date, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonRole, body, "role", JTokenType.String, ref responseModel, true))
                    {
                        break;
                    }

                    string password = jsonPassword?.ToString();
                    string phoneNumber = jsonPhonenumber?.ToString();
                    string address = jsonAddress?.ToString();
                    string avatar = jsonAvatar?.ToString();
                    string slogan = jsonSlogan?.ToString();
                    string bio = jsonBio?.ToString();
                    string job = jsonJob?.ToString();
                    string gender = jsonGender?.ToString();
                    DateTime.TryParse(jsonBirthday?.ToString(), out DateTime birthday);
                    string roleType = RoleType.TryParse(jsonRole?.ToString());

                    User user = new User();
                    user.Password = password ?? user.Password;
                    user.PhoneNumber = phoneNumber ?? user.Password;
                    user.Address = address ?? user.Password;
                    user.Avatar = avatar ?? user.Password;
                    user.Slogan = slogan ?? user.Password;
                    user.Password = password ?? user.Password;
                    user.Role = roleType ?? user.Role;

                    if (!_userService.TryAddUser(user))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                    }
                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray { JObject.FromObject(user) };

                } while (false);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 501;
                responseModel.ErrorCode = 501;
                responseModel.Message = ex.Message;
            }

            return Ok(responseModel.ToString());
        }

        [HttpPut("users/{userId}")]
        public IActionResult UpdateUser(int userId, [FromBody]object requestBody)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                do
                {
                    JObject body = requestBody != null
                        ? JObject.Parse(requestBody.ToString())
                        : null;

                    JArray data = new JArray();

                    if (!CoreHelper.GetParameter(out JToken jsonPassword, body, "password", JTokenType.String, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonPhonenumber, body, "phonenumber", JTokenType.String, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonAddress, body, "address", JTokenType.String, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonAvatar, body, "avatar", JTokenType.String, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonSlogan, body, "slogan", JTokenType.String, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonBio, body, "bio", JTokenType.String, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonJob, body, "job", JTokenType.String, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonGender, body, "gender", JTokenType.String, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonBirthday, body, "birthday", JTokenType.Date, ref responseModel, true)
                        || !CoreHelper.GetParameter(out JToken jsonRole, body, "role", JTokenType.String, ref responseModel, true))
                    {
                        break;
                    }

                    string password = jsonPassword?.ToString();
                    string phoneNumber = jsonPhonenumber?.ToString();
                    string address = jsonAddress?.ToString();
                    string avatar = jsonAvatar?.ToString();
                    string slogan = jsonSlogan?.ToString();
                    string bio = jsonBio?.ToString();
                    string job = jsonJob?.ToString();
                    string gender = jsonGender?.ToString();
                    DateTime.TryParse(jsonBirthday?.ToString(), out DateTime birthday);
                    string roleType = RoleType.TryParse(jsonRole?.ToString());

                    if (!_userService.TryGetUsers(userId, out User user))
                    {
                        break;
                    }

                    bool isSuccess = false;
                    if (user == null)
                    {
                        user = new User();
                        user.Password = password ?? user.Password;
                        user.PhoneNumber = phoneNumber ?? user.Password;
                        user.Address = address ?? user.Password;
                        user.Avatar = avatar ?? user.Password;
                        user.Slogan = slogan ?? user.Password;
                        user.Password = password ?? user.Password;
                        user.Role = roleType ?? user.Role;

                        isSuccess = _userService.TryAddUser(user);
                    }
                    else
                    {
                        user.Password = password ?? user.Password;
                        user.PhoneNumber = phoneNumber ?? user.Password;
                        user.Address = address ?? user.Password;
                        user.Avatar = avatar ?? user.Password;
                        user.Slogan = slogan ?? user.Password;
                        user.Password = password ?? user.Password;
                        user.Role = roleType ?? user.Role;

                        isSuccess = _userService.TryUpdateUser(user);
                    }
                   
                    if (!isSuccess)
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                    }
                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray { JObject.FromObject(user) };

                } while (false);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 501;
                responseModel.ErrorCode = 501;
                responseModel.Message = ex.Message;
            }

            return Ok(responseModel.ToString());
        }

        [HttpGet("tour-infos/{id}")]
        public IActionResult GetTourInfo(int id)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                do
                {
                    if (!_userService.TryGetTourInfoById(id, out TourInfo tourInfo))
                    {
                        break;
                    }

                    if (tourInfo == null)
                    {
                        responseModel.ErrorCode = (int)ErrorList.ErrorCode.UserNotFound;
                        responseModel.Message = ErrorList.Description(responseModel.ErrorCode);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray { JObject.FromObject(tourInfo) };
                } while (false);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 501;
                responseModel.ErrorCode = 501;
                responseModel.Message = ex.Message;
            }

            return Ok(responseModel.ToString());
        }


        [HttpPut("tour-infos/{id}")]
        public IActionResult UpdateTourInfo(int id, [FromBody]object requestBody)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                do
                {
                    if (!_userService.TryGetTourInfoById(id, out TourInfo tourInfo))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
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

                    if (_userService.TryUpdateTourInfo(tourInfo))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray { JObject.FromObject(tourInfo) };

                } while (false);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 501;
                responseModel.ErrorCode = 501;
                responseModel.Message = ex.Message;
            }

            return Ok(responseModel.ToString());
        }


        [HttpDelete("tour-infos/{id}")]
        public IActionResult DeleteTourInfo(int id)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                do
                {
                    if (!_userService.TryRemoveTourInfo(id))
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

            return Ok(responseModel.ToString());
        }

        private JObject UserResponseJson(User user)
        {
            JObject jObject = new JObject();
            jObject.Add("Id", user.Id);
            jObject.Add("Token", user.Token);

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

        //    return Ok(responseModel.ToString());
        //}
    }
}
