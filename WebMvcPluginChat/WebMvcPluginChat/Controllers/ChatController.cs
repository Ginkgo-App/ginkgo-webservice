using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using APICore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

        [HttpGet]
        public object GetAllGroupChat([FromQuery] int page, [FromQuery] int pageSize)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    var errorCode = _chatService.GetAllGroupChat(page, pageSize, userId, out var groups, out var pagination);

                    if (!errorCode)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = JArray.FromObject(groups);
                    responseModel.AdditionalProperties["Pagination"] = JObject.FromObject(pagination);
                }
                while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpPost("group")]
        public object CreateGroupChat([FromBody]object requestBody)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var body = requestBody != null
                        ? JObject.Parse(requestBody.ToString() ?? "{}")
                        : null;

                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    if (!CoreHelper.GetParameter(out var jsonName, body, "Name", JTokenType.String,
                            ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonAvatar, body, "Avatar", JTokenType.String,
                            ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonMembers, body, "Members", JTokenType.Array,
                            ref responseModel, true))
                    {
                        break;
                    }
                    var errorCode = _chatService.CreateGroupChat(
                        userId, jsonName.ToString(),
                        jsonMembers.ToObject<List<int>>(),
                        jsonAvatar.ToString(),
                        out var group);

                    if (!errorCode)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = new JArray(JObject.FromObject(group));
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
        public object SendMessage([FromBody] object requestBody)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var body = requestBody != null
                        ? JObject.Parse(requestBody.ToString() ?? "{}")
                        : null;

                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    if (!CoreHelper.GetParameter(out var jsonGroupId, body, "GroupId", JTokenType.Integer,
                            ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonContent, body, "Content", JTokenType.String,
                            ref responseModel)
                        || !CoreHelper.GetParameter(out var jsonMembers, body, "Images", JTokenType.Array,
                            ref responseModel, true))
                    {
                        break;
                    }

                    var message = new Message
                    {
                        GroupId = int.Parse(jsonGroupId.ToString()),
                        Content = jsonContent.ToString(),
                        Images = jsonMembers?.ToObject<string[]>(),
                        CreateAt = DateTime.UtcNow,
                        CreateBy = userId,
                    };

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

        [HttpGet("{groupId}/messages")]
        public object GetAllMessage(int groupId, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    var errorCode = _chatService.GetAllMessagesOfGroup(page, pageSize, userId, groupId, out var messages, out var pagination);

                    if (!errorCode)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = JArray.FromObject(messages);
                    responseModel.AdditionalProperties["Pagination"] = JObject.FromObject(pagination);
                }
                while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpGet("tour/{groupId}")]
        public object TryGetTourGroupChat(int groupId)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    var errorCode = _chatService.TryGetTourGroupChat(userId, groupId, out var groupInfo);

                    if (!errorCode)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = new JArray { JObject.FromObject(groupInfo) };
                }
                while (false);
            }
            catch (Exception ex)
            {
                responseModel.FromException(ex);
            }

            return responseModel.ToJson();
        }

        [HttpGet("user/{userId}")]
        public object TryGetUserChat(int userId)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var myId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    var errorCode = _chatService.TryGetUserChat(myId, userId, out var groupInfo);

                    if (!errorCode)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = new JArray { JObject.FromObject(groupInfo) };
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