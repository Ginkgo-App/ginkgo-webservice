using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
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
                data.Add(user.Token);
                //data.Add(JObject.Parse(JsonConvert.SerializeObject(userHelper.WithoutPassword(user))));
                if (user == null || errorCode != ErrorCode.Success)
                {
                    responseModel.ErrorCode = (int)ErrorCode.UsernamePasswordIncorrect;
                    responseModel.Message = Description(responseModel.ErrorCode);
                }
                else
                {
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
                        data.Add(user.Token);
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

        [HttpGet("{userId}")]
        public IActionResult GetUser(string userId)
        {
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("data")]
        public IActionResult Data()
        {
            try
            {

                return Ok("Success");
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
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
                        || !CoreHelper.GetParameter(out JToken jsonEmail, body, "Email", JTokenType.String, ref responseModel))
                    {
                        break;
                    }

                    string accessToken = jsonAccessToken.ToString();
                    string type = jsonType.ToString();
                    string email = jsonEmail.ToString();

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
                            data.Add(user.Token);
                            responseModel.ErrorCode = (int)ErrorCode.Success;
                            responseModel.Message = Description(responseModel.ErrorCode);
                            responseModel.Data = data;
                        }
                        else
                        {
                            responseModel.FromErrorCode(ErrorCode.Fail);
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
    }
}
