using APICore.DBContext;
using APICore.Entities;
using APICore.Helpers;
using APICore.Middlewares;
using APICore.Models;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace APICore.Services
{
    public class NotificationService
    {
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.Logger;
        private PostgreSQLContext _context;
        private readonly FriendService _friendService;
        private ConnectionManager _connectionManager;

        public bool GetNotification(int userId, int page, int pageSize, out User user, out List<NotificationInfo> notifications, out Pagination pagination)
        {
            notifications = new List<NotificationInfo>();
            pagination = null;
            bool isSuccess;
            ConcurrentBag<GroupInfo> groupInfos = new ConcurrentBag<GroupInfo>();
            notifications = new List<NotificationInfo>();
            user = null;

            try
            {
                CoreHelper.ValidatePageSize(ref page, ref pageSize);

                DbService.ConnectDb(out _context);

                var listTourIds = _context.TourMembers.Where(x => x.UserId == userId).Select(x => x.TourId).ToList();
                var listGrouIds = _context.UserGroup.Where(x => x.UserId == userId).Select(x => x.GroupId).ToList();

                var listGroup = _context.Groups.Where(g => listGrouIds.Contains(g.ID) || listTourIds.Contains(g.TourId))
                    .OrderBy(g => g.LastMessageAt).ToList();

                var total = listGroup.Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    // If pageSize = 0 => Get all
                    listGroup = pageSize <= 0
                        ? listGroup
                        : listGroup
                            .Skip(skip)
                            .Take(pageSize)
                            .ToList();

                    ConcurrentBag<GroupInfo> groupBags = new ConcurrentBag<GroupInfo>();

                    ////Parallel.ForEach(listGroup, (group) =>
                    foreach (var group in listGroup)
                    {
                        ////var groupContext = _context.Groups.FirstOrDefault(x => x.ID == group.Group);
                        var tourMemberIds = _context.TourMembers.Where(m => m.TourId == group.TourId);
                        var memberIds = _context.UserGroup.Where(x => x.GroupId == group.ID);
                        var members = tourMemberIds.Count() > 0
                            ? _context.Users.Where(x => tourMemberIds.FirstOrDefault(id => id.UserId == x.Id) != null).ToList()
                            : _context.Users.Where(x => memberIds.FirstOrDefault(id => id.UserId == x.Id) != null).ToList();
                        var lastSeenMessageId = _context.UserGroup.FirstOrDefault(x => x.GroupId == group.ID && x.UserId == userId)?.LastSeenMessageId ?? 0;

                        var lastMessage = _context.Messages.FirstOrDefault(x => x.Id == group.LastMessageId);
                        var lastSeenMessageAt = _context.Messages.FirstOrDefault(x => x.Id == lastSeenMessageId)?.CreateAt ?? DateTime.MinValue;

                        var unseenMessages = _context.Messages.Where(x => x.GroupId == group.ID && x.CreateAt > lastSeenMessageAt);


                        var memInfo = new List<SimpleUser>();

                        for (int i = 0; i < members.Count(); i++)
                        {
                            memInfo.Add(ConvertToSimpleUser(members[i], userId));
                        }

                        var otherUser = memInfo.FirstOrDefault(x => x.Id != userId);

                        var groupInfo = new GroupInfo
                        (
                            group.ID,
                            group.GroupName,
                            memInfo,
                            group.Avatar,
                            lastMessage
                        );

                        groupInfo.Avatar = string.IsNullOrEmpty(groupInfo.Avatar)
                            ? otherUser?.Avatar ?? groupInfo.Avatar
                            : groupInfo.Avatar;

                        groupInfo.Name = string.IsNullOrEmpty(groupInfo.Name)
                            ? otherUser?.Name ?? groupInfo.Name
                            : groupInfo.Name;

                        groupInfo.UnreadCount = unseenMessages.Count();
                        groupInfo.OtherUser = otherUser;
                        groupInfo.OtherUserId = otherUser?.Id ?? 0;
                        groupInfo.TourId = group.TourId;

                        groupBags.Add(groupInfo);
                        //});
                    }

                    //groups.AddRange(groupBags);
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
