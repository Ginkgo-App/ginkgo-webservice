using APICore.Helpers;
using APICore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using WebMvcPluginUser.DBContext;
using WebMvcPluginUser.Entities;
using WebMvcPluginUser.Helpers;
using WebMvcPluginUser.Models;
using WebMvcPluginUser.Services;
using static APICore.Helpers.ErrorList;

namespace WebMvcPluginUser.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/" +UserVars.Version + "/users")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            UserHelper userHelper = new UserHelper();
            JArray data = new JArray();

            _userService.Authenticate(model.Username, model.Password, out User user);
            data.Add(JObject.Parse(JsonConvert.SerializeObject(userHelper.WithoutPassword(user))));

            if (user == null)
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

            return Ok(responseModel.ToString());
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]object requestBody)
        {
            return Ok("Success");
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
    }
}
