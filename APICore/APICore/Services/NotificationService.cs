using APICore.DBContext;
using APICore.Entities;
using APICore.Helpers;
using APICore.Middlewares;
using APICore.Models;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace APICore.Services
{
    public interface INotificationService
    {
        string CalculateIsFriend(int userId, int userRequestId);
        bool CreateNotification(int userId, List<Notification> notifications, bool isSend = true);
        bool GetNotification(int userId, int page, int pageSize, out User user, out List<NotificationInfo> notifications, out Pagination pagination);
    }

    public class NotificationService : INotificationService
    {
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.Logger;
        private PostgreSQLContext _context;
        private readonly FriendService _friendService;
        private ConnectionManager _connectionManager;
        private OneSignalService _oneSignalService;

        public bool GetNotification(int userId, int page, int pageSize, out User user, out List<NotificationInfo> notifications, out Pagination pagination)
        {
            notifications = new List<NotificationInfo>();
            pagination = null;
            bool isSuccess;
            notifications = new List<NotificationInfo>();
            user = null;

            try
            {
                CoreHelper.ValidatePageSize(ref page, ref pageSize);

                DbService.ConnectDb(out _context);

                var listNotifications = _context.Notifications.Where(x => x.ReceiverId == userId).OrderByDescending(x => x.SendAt).ToList();

                var total = listNotifications.Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    // If pageSize = 0 => Get all
                    listNotifications = pageSize <= 0
                        ? listNotifications
                        : listNotifications
                            .Skip(skip)
                            .Take(pageSize)
                            .ToList();

                    ConcurrentBag<NotificationInfo> notiInfos = new ConcurrentBag<NotificationInfo>();

                    ////Parallel.ForEach(listGroup, (group) =>
                    foreach (var noti in listNotifications)
                    {
                        var notoInfo = JObject.FromObject(noti).ToObject<NotificationInfo>();
                        var senderInfo = _context.Users.FirstOrDefault(m => m.Id == noti.SenderId);
                        notoInfo.SenderName = senderInfo?.Name ?? "Không có tên";

                        notiInfos.Add(notoInfo);
                    }

                    notifications.Clear();
                    notifications.AddRange(notiInfos);
                }
                else
                {
                    //groups = new List<GroupInfo>();
                }

                pagination = new Pagination(total, page, pageSize > 0 ? pageSize : total);
                isSuccess = true;
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return isSuccess;
        }

        public bool CreateNotification(int userId, List<Notification> notifications, bool isSend = true)
        {
            try
            {
                DbService.ConnectDb(out _context);

                foreach (var noti in notifications)
                {
                    noti.SendAt = DateTime.UtcNow;
                    noti.SenderId = userId;

                    _context.Notifications.Add(noti);
                    _context.SaveChanges();

                    var notiJson = JObject.FromObject(noti);
                    _oneSignalService.SendNotification(new int[] { noti.ReceiverId }, noti.Title, notiJson.ToString());
                }
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }


        public string CalculateIsFriend(int userId, int userRequestId)
        {
            if (userId == userRequestId)
            {
                return FriendType.Me;
            }

            TryGetFriendRequest(userId, userRequestId, out var friendDb);
            if (friendDb == null)
            {
                return FriendType.None;
            }

            if (friendDb.AcceptedAt != null)
            {
                return FriendType.Accepted;
            }

            if (userId == friendDb.UserId)
            {
                return FriendType.Waiting;
            }

            return userId == friendDb.RequestedUserId ? FriendType.Requested : FriendType.None;
        }

        private void TryGetFriendRequest(int userId, int userOtherId, out Friend friendDb)
        {
            DbService.ConnectDb(out _context);
            var friendDBs = _context.Friends.Where(a =>
                    (a.UserId == userId && a.RequestedUserId == userOtherId && a.DeletedAt == null)
                    || (a.RequestedUserId == userId && a.UserId == userOtherId && a.DeletedAt == null))
                .ToArray();

            friendDb = friendDBs.Length > 0 ? friendDBs[0] : null;
        }
        private SimpleUser ConvertToSimpleUser(User user, int userId)
        {
            DbService.ConnectDb(out _context);

            var friendType = CalculateIsFriend(userId, user.Id);
            var totalPost = _context.Posts.Where(p => p.AuthorId == user.Id).Count();

            return new SimpleUser
            (
                user.Id,
                user.Name,
                user.Avatar,
                user.Job,
                friendType,
                totalPost
            );
        }

    }
}
