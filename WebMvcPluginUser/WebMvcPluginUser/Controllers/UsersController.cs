using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [Authorize]
    [ApiController]
    [Route("api/" + UserVars.Version + "/users")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public string Authenticate([FromBody]AuthenticateModel model)
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
                    responseModel.ErrorCode = (int)ErrorCode.UsernamePasswordIncorrect;
                    responseModel.Message = Description(responseModel.ErrorCode);
                }
                else
                {
                    data.Add(UserResponseJson(user));

                    responseModel.ErrorCode = (int)ErrorCode.Success;
                    responseModel.Message = Description(responseModel.ErrorCode);
                    responseModel.Data = data;
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = 501;
                responseModel.ErrorCode = 501;
                responseModel.Message = ex.Message;
            }

            return responseModel.ToString();
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]object requestBody)
        {
            ResponseModel response = new ResponseModel();
            User user = null;
            try
            {
                do
                {
                    JArray data = new JArray();

                    // Parse request body to json
                    JObject reqBody = requestBody != null
                        ? JObject.Parse(requestBody.ToString())
                        : null;

                    if (!CoreHelper.GetParameter(out JToken jsonName, reqBody, "Name", JTokenType.String, ref response)
                        || !CoreHelper.GetParameter(out JToken jsonPhonenumber, reqBody, "PhoneNumber", JTokenType.String, ref response)
                        || !CoreHelper.GetParameter(out JToken jsonEmail, reqBody, "Email", JTokenType.String, ref response)
                        || !CoreHelper.GetParameter(out JToken jsonPassword, reqBody, "Password", JTokenType.String, ref response)
                        )
                    {
                        break;
                    }

                    string name = jsonName.ToString();
                    string email = jsonEmail.ToString();
                    string phonenumber = jsonPhonenumber.ToString();
                    string hashedPassword = UserHelper.HashPassword(jsonPassword.ToString());

                    var statusCode = _userService.Register(name, email, phonenumber, hashedPassword, out user);

                    if (statusCode == ErrorCode.Success)
                    {
                        data.Add(UserResponseJson(user));
                    }

                    response.ErrorCode = (int)statusCode;
                    response.Message = ErrorList.Description(response.ErrorCode);
                    response.Data = data;

                } while (false);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 501;
                response.ErrorCode = 501;
                response.Message = ex.Message;
            }
            return Ok(response.ToString());
        }

        [AllowAnonymous]
        [HttpPost("social-provider")]
        public IActionResult SocialProvider([FromBody]object requestBody)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                do
                {
                    
                    JObject body = requestBody != null
                        ? JObject.Parse(requestBody.ToString())
                        : null;



                    if (!CoreHelper.GetParameter(out JToken jsonAccessToken, body, "AccessToken", JTokenType.String, ref responseModel)
                        || !CoreHelper.GetParameter(out JToken jsonType, body, "Type", JTokenType.String, ref responseModel)
                        || !CoreHelper.GetParameter(out JToken jsonEmail, body, "Email", JTokenType.String, ref responseModel, isNullable: true))
                    {
                        break;
                    }

                    string accessToken = jsonAccessToken.ToString();
                    string type = jsonType.ToString();
                    string email = (jsonEmail ?? "").ToString();

                    if (type.Equals("facebook", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!_userService.TryGetFacbookInfo(accessToken, out AuthProvider authProvider))
                        {
                            break;
                        }

                        JArray data = new JArray();

                        ErrorCode errorCode = _userService.Authenticate(email, ref authProvider, out User user);

                        if (errorCode == ErrorCode.Success)
                        {
                            data.Add(UserResponseJson(user));
                            responseModel.ErrorCode = (int)ErrorCode.Success;
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
                Response.StatusCode = 501;
                responseModel.ErrorCode = 501;
                responseModel.Message = ex.Message;
            }

            return Ok(responseModel.ToString());
        }

        [Authorize(Roles = RoleType.Admin)]
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

        [HttpGet("{userId}")]
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

        [HttpDelete("{userId}")]
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

        [HttpPost("{userId}")]
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
                        || !CoreHelper.GetParameter(out JToken jsonBirthday, body, "birthday", JTokenType.Date, ref responseModel, true))
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

                    if (!_userService.TryGetUsers(userId, out User user))
                    {
                        break;
                    }

                    user.Password = password ?? user.Password;
                    user.PhoneNumber = phoneNumber ?? user.Password;
                    user.Address = address ?? user.Password;
                    user.Avatar = avatar ?? user.Password;
                    user.Slogan = slogan ?? user.Password;
                    user.Password = password ?? user.Password;

                    bool isSuccess = _userService.TryUpdateUser(user);

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
