using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using APICore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace WebMvcPluginChat.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/" + PlaceVars.Version + "/chats")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public object CreateGroupChat([FromBody]GroupInfo groupInfo)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    var errorCode = _chatService.CreateGroupChat(
                        userId, groupInfo.Name,
                        groupInfo.Members.Select(x => x.Id).ToList(),
                        groupInfo.Avatar);

                    if (!errorCode)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = new JArray();
                }
                while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpPost("message")]
        public object SendMessage([FromBody]Message message)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    var errorCode = _chatService.SendMessage(userId, message);

                    if (!errorCode)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = new JArray();
                }
                while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

    }
}