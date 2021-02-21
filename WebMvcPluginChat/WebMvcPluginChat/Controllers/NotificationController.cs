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
using System.Threading.Tasks;

namespace WebMvcPluginChat.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/" + PlaceVars.Version + "/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly INotificationService _notificationService;

        public NotificationController(IChatService chatService, INotificationService notificationService)
        {
            _chatService = chatService;
            _notificationService = notificationService;
        }

        [HttpGet]
        public object GetNotifications([FromQuery] int page, [FromQuery] int pageSize)
        {
            var responseModel = new ResponseModel();

            try
            {
                do
                {
                    var userId = CoreHelper.GetUserId(HttpContext, ref responseModel);

                    var errorCode = _notificationService.GetNotification(userId, page, pageSize, out var user, out var notifications, out var pagination);

                    if (!errorCode)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = JArray.FromObject(notifications);
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
        public object CreateGroupChat([FromBody] object requestBody)
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

                    var errorCode = _chatService.SendMessage(userId, message, out var sender, out var group, out var members);

                    if (!errorCode)
                    {
                        responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
                        break;
                    }

                    responseModel.FromErrorCode(ErrorList.ErrorCode.Success);
                    responseModel.Data = new JArray();

                    Task.Factory.StartNew(() =>
                    {
                        _notificationService.CreateNotification(userId, members.Where(x => !x.Equals(userId)).Select(memberId => new Notification
                        {
                            Title = "Tin nhắn mới",
                            Message = $"Bạn có tin nhắn mới từ {sender?.Name}",
                            Type = "message",
                            ReceiverId = memberId,
                        }).ToList()); 
                    });
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