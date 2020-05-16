using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using APICore.Services;
using static APICore.Helpers.ErrorList;

namespace APICore.Controllers
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

        [HttpGet("users")]
        public object GetAllUser([FromQuery]int page, [FromQuery]int pageSize)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                do
                {
                    List<User> users = null;
                    Pagination pagination = null;
                    JArray data = new JArray();

                    if (!_userService.TryGetUsers(page, pageSize, out users, out pagination))
                    {
                        break;
                    }

                    if (users == null || users.Count == 0 || pagination == null)
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

        [HttpGet("users/{userId}")]
        public object GetUserById(int userId)
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

            return responseModel.ToJson();
        }

        [HttpDelete("users/{userId}")]
        public object DeleteUser(int userId)
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

            return responseModel.ToJson();
        }

        [HttpPost("users")]
        public object AddUser([FromBody]object requestBody)
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
                        || !CoreHelper.GetParameter(out JToken jsonEmail, body, "email", JTokenType.String, ref responseModel)
                        || !CoreHelper.GetParameter(out JToken jsonName, body, "name", JTokenType.String, ref responseModel, true)
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

                    string name = jsonName?.ToString();
                    string email = jsonEmail.ToString();
                    string password = UserHelper.HashPassword(jsonPassword?.ToString());
                    string phoneNumber = jsonPhonenumber?.ToString();
                    string address = jsonAddress?.ToString();
                    string avatar = jsonAvatar?.ToString();
                    string slogan = jsonSlogan?.ToString();
                    string bio = jsonBio?.ToString();
                    string job = jsonJob?.ToString();
                    string gender = jsonGender?.ToString();
                    bool isParsed = DateTime.TryParse(jsonBirthday?.ToString(), out DateTime birthday);
                    string roleType = RoleType.TryParse(jsonRole?.ToString());

                    User user = new User();
                    user.Name  = name ?? user.Name;
                    user.Email  = email ?? user.Email;
                    user.Password = password ?? user.Password;
                    user.PhoneNumber = phoneNumber ?? user.Password;
                    user.Address = address ?? user.Password;
                    user.Avatar = avatar ?? user.Password;
                    user.Slogan = slogan ?? user.Password;
                    user.Bio = bio ?? user.Bio;
                    user.Job = job ?? user.Job;
                    user.Gender = GenderType.TryParse(gender) ?? user.Gender;
                    user.Birthday = isParsed ? birthday : user.Birthday;
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

            return responseModel.ToJson();
        }

        [HttpPut("users/{userId}")]
        public object UpdateUser(int userId, [FromBody]object requestBody)
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
                        || !CoreHelper.GetParameter(out JToken jsonEmail, body, "email", JTokenType.String, ref responseModel)
                        || !CoreHelper.GetParameter(out JToken jsonName, body, "name", JTokenType.String, ref responseModel, true)
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

                    string name = jsonName?.ToString();
                    string email = jsonEmail.ToString();
                    string password = UserHelper.HashPassword(jsonPassword?.ToString());
                    string phoneNumber = jsonPhonenumber?.ToString();
                    string address = jsonAddress?.ToString();
                    string avatar = jsonAvatar?.ToString();
                    string slogan = jsonSlogan?.ToString();
                    string bio = jsonBio?.ToString();
                    string job = jsonJob?.ToString();
                    string gender = jsonGender?.ToString();
                    bool isParsed = DateTime.TryParse(jsonBirthday?.ToString(), out DateTime birthday);
                    string roleType = RoleType.TryParse(jsonRole?.ToString());


                    if (!_userService.TryGetUsers(userId, out User user))
                    {
                        break;
                    }

                    bool isSuccess = false;
                    if (user == null)
                    {
                        user = new User();
                        user.Name = name ?? user.Name;
                        user.Email = email ?? user.Email;
                        user.Password = password ?? user.Password;
                        user.PhoneNumber = phoneNumber ?? user.Password;
                        user.Address = address ?? user.Password;
                        user.Avatar = avatar ?? user.Password;
                        user.Slogan = slogan ?? user.Password;
                        user.Bio = bio ?? user.Bio;
                        user.Job = job ?? user.Job;
                        user.Gender = GenderType.TryParse(gender) ?? user.Gender;
                        user.Birthday = isParsed ? birthday : user.Birthday;
                        user.Role = roleType ?? user.Role;

                        isSuccess = _userService.TryAddUser(user);
                    }
                    else
                    {
                        user.Name = name ?? user.Name;
                        user.Email = email ?? user.Email;
                        user.Password = password ?? user.Password;
                        user.PhoneNumber = phoneNumber ?? user.Password;
                        user.Address = address ?? user.Password;
                        user.Avatar = avatar ?? user.Password;
                        user.Slogan = slogan ?? user.Password;
                        user.Bio = bio ?? user.Bio;
                        user.Job = job ?? user.Job;
                        user.Gender = GenderType.TryParse(gender) ?? user.Gender;
                        user.Birthday = isParsed ? birthday : user.Birthday;
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

            return responseModel.ToJson();
        }

        [HttpGet("tour-infos/{id}")]
        public object GetTourInfo(int id)
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

            return responseModel.ToJson();
        }

        [HttpPut("tour-infos/{id}")]
        public object UpdateTourInfo(int id, [FromBody]object requestBody)
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

            return responseModel.ToJson();
        }

        [HttpDelete("tour-infos/{id}")]
        public object DeleteTourInfo(int id)
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

            return responseModel.ToJson();
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

        //    return responseModel.ToJson();
        //}
    }
}
