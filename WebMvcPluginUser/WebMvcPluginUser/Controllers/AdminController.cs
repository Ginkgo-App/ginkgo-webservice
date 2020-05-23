﻿using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using APICore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using WebMvcPluginUser.Helpers;
using static APICore.Helpers.ErrorList;

namespace WebMvcPluginUser.Controllers
{
    [Authorize(Roles = RoleType.Admin)]
    [ApiController]
    [Route("api/" + UserVars.Version + "/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("users")]
        public object GetAllUser([FromQuery] int page, [FromQuery] int pageSize)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                do
                {
                    var data = new JArray();

                    if (!_userService.TryGetUsers(page, pageSize, out var users, out var pagination))
                    {
                        break;
                    }

                    if (users == null || users.Count == 0 || pagination == null)
                    {
                        responseModel.ErrorCode = (int)ErrorCode.UserNotFound;
                        responseModel.Message = Description(responseModel.ErrorCode);
                        break;
                    }

                    foreach (var user in users)
                    {
                        data.Add(JObject.FromObject(user));
                    }

                    responseModel.ErrorCode = (int)ErrorCode.Success;
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

        [HttpGet("users/{userId}")]
        public object GetUserById(int userId)
        {
            ResponseModel responseModel = new ResponseModel();

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
                        responseModel.ErrorCode = (int)ErrorCode.UserNotFound;
                        responseModel.Message = Description(responseModel.ErrorCode);
                        break;
                    }

                    data.Add(JObject.FromObject(user));
                    responseModel.ErrorCode = (int)ErrorCode.Success;
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
                    responseModel.Message = Description(responseModel.ErrorCode);
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpPost("users")]
        public object AddUser([FromBody] object requestBody)
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
                        || !CoreHelper.GetParameter(out var jsonEmail, body, "email", JTokenType.String,
                            ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonName, body, "name", JTokenType.String,
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
                            ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonRole, body, "role", JTokenType.String,
                            ref responseModel, true))
                    {
                        break;
                    }

                    var name = jsonName?.ToString();
                    var email = jsonEmail.ToString();
                    var password = UserHelper.HashPassword(jsonPassword?.ToString());
                    var phoneNumber = jsonPhoneNumber?.ToString();
                    var address = jsonAddress?.ToString();
                    var avatar = jsonAvatar?.ToString();
                    var slogan = jsonSlogan?.ToString();
                    var bio = jsonBio?.ToString();
                    var job = jsonJob?.ToString();
                    var gender = jsonGender?.ToString();
                    var isParsed = DateTime.TryParse(jsonBirthday?.ToString(), out DateTime birthday);
                    var roleType = RoleType.TryParse(jsonRole?.ToString());

                    User user = new User();
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
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpPut("users/{userId}")]
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
                        || !CoreHelper.GetParameter(out var jsonEmail, body, "email", JTokenType.String,
                            ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonName, body, "name", JTokenType.String,
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
                            ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonRole, body, "role", JTokenType.String,
                            ref responseModel, true))
                    {
                        break;
                    }

                    var name = jsonName?.ToString();
                    var email = jsonEmail.ToString();
                    var password = UserHelper.HashPassword(jsonPassword?.ToString());
                    var phoneNumber = jsonPhoneNumber?.ToString();
                    var address = jsonAddress?.ToString();
                    var avatar = jsonAvatar?.ToString();
                    var slogan = jsonSlogan?.ToString();
                    var bio = jsonBio?.ToString();
                    var job = jsonJob?.ToString();
                    var gender = jsonGender?.ToString();
                    var isParsed = DateTime.TryParse(jsonBirthday?.ToString(), out DateTime birthday);
                    var roleType = RoleType.TryParse(jsonRole?.ToString());


                    if (!_userService.TryGetUsers(userId, out User user))
                    {
                        break;
                    }

                    bool isSuccess;
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
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpGet("tour-infos/{id}")]
        public object GetTourInfo(int id)
        {
            var responseModel = new ResponseModel();

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
                        responseModel.ErrorCode = (int)ErrorCode.UserNotFound;
                        responseModel.Message = Description(responseModel.ErrorCode);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorCode.Success);
                    responseModel.Data = new JArray { JObject.FromObject(tourInfo) };
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpPut("tour-infos/{id}")]
        public object UpdateTourInfo(int id, [FromBody] object requestBody)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    if (!_userService.TryGetTourInfoById(id, out TourInfo tourInfo))
                    {
                        responseModel.FromErrorCode(ErrorCode.Fail);
                        break;
                    }

                    var body = requestBody != null
                        ? JObject.Parse(requestBody.ToString()!)
                        : null;

                    if (!CoreHelper.GetParameter(out var jsonDestinationPlaceId, body, "DestinationPlaceId",
                            JTokenType.Integer, ref responseModel, true)
                        || !CoreHelper.GetParameter(out var jsonStartPlaceId, body, "StartPlaceId", JTokenType.Integer,
                            ref responseModel, true)
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
                    tourInfo.DestinatePlaceId = isDestinationPlaceId ? destinationPlaceId : tourInfo.StartPlaceId;

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
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpDelete("tour-infos/{id}")]
        public object DeleteTourInfo(int id)
        {
            var responseModel = new ResponseModel();

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
                    responseModel.Message = Description(responseModel.ErrorCode);
                } while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
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