using APICore.DBContext;
using APICore.Entities;
using APICore.Helpers;
using APICore.Middlewares;
using APICore.Models;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace APICore.Services
{
    public interface IChatService
    {
        bool AddUsersToGroup(int userId, int[] addUserIds, int groupId, List<int> members);
        string CalculateIsFriend(int userId, int userRequestId);
        bool CreateGroupChat(int userId, string groupName, List<int> members, string avatar, out Group group);
        bool GetAllGroupChat(int page, int pageSize, int userId, out List<GroupInfo> groups, out Pagination pagination);
        bool GetAllMessagesOfGroup(int page, int pageSize, int userId, int groupId, out List<MessageInfo> messages, out Pagination pagination);
        bool RemoveUsersInGroup(int userId, int[] removeUserIds, int groupId, List<int> members);
        bool SendMessage(int userId, Message message);
        bool TryGetTourGroupChat(int userId, int tourId, out GroupInfo group);
        bool TryGetUserChat(int userId, int otherUserId, out GroupInfo group);
    }

    public class ChatService : IChatService
    {
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.Logger;
        private PostgreSQLContext _context;
        private readonly FriendService _friendService;
        private ConnectionManager _connectionManager;

        public ChatService(IOptions<AppSettings> appSettings, ConnectionManager connectionManager)
        {
            _appSettings = appSettings.Value;
            _friendService = new FriendService(appSettings);
            _connectionManager = connectionManager;
        }

        public bool GetAllGroupChat(int page, int pageSize, int userId, out List<GroupInfo> groups,
            out Pagination pagination)
        {
            groups = new List<GroupInfo>();
            pagination = null;
            bool isSuccess;
            ConcurrentBag<GroupInfo> groupInfos = new ConcurrentBag<GroupInfo>();

            try
            {
                CoreHelper.ValidatePageSize(ref page, ref pageSize);

                DbService.ConnectDb(out _context);

                ////var listGroup = (
                ////    from ug in _context.UserGroup
                ////    join g in _context.Groups
                ////        on ug.GroupId equals g.ID
                ////    where ug.ID == userId
                ////    group ug by ug.GroupId into gc
                ////    select new
                ////    {
                ////        Group = gc.Key,
                ////        Members = gc.Count(),
                ////    })?.AsEnumerable().ToList();

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

                    groups.AddRange(groupBags);
                    //groups.Add(new ListGroupInfo(group.Group, "Test", group.Members.Select(x=>x.UserName));
                }
                else
                {
                    groups = new List<GroupInfo>();
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

        public bool TryGetTourGroupChat(int userId, int tourId, out GroupInfo group)
        {
            bool isSuccess;

            try
            {
                DbService.ConnectDb(out _context);

                var tour = _context.Tours.FirstOrDefault(t => t.Id == tourId);
                var tourInfo = _context.TourInfos.FirstOrDefault(t => t.Id == tour.TourInfoId);

                if (tour == null || tourInfo == null)
                {
                    throw new ExceptionWithMessage("Không tìm thấy tour");
                }

                var groupContext = _context.Groups.FirstOrDefault(g => g.TourId == tourId);

                if (groupContext == null)
                {
                    groupContext = new Group
                    {
                        Avatar = tourInfo.Images.FirstOrDefault() ?? string.Empty,
                        CreatorId = userId,
                        GroupName = tour.Name,
                        LastMessageAt = DateTime.MinValue,
                        LastMessageId = 0,
                        LastUpdate = DateTime.UtcNow,
                        TourId = tourId,
                    };

                    _context.Groups.Add(groupContext);
                    _context.SaveChanges();
                }

                groupContext.Avatar = string.IsNullOrEmpty(groupContext.Avatar)
                    ? tourInfo.Images.FirstOrDefault()
                    : groupContext.Avatar;

                var tourMemberIds = _context.TourMembers.Where(m => m.TourId == tourId);
                var memberIds = _context.UserGroup.Where(x => x.GroupId == groupContext.ID);
                var members = tourMemberIds.Count() > 0
                    ? _context.Users.Where(x => tourMemberIds.FirstOrDefault(id => id.UserId == x.Id) != null).ToList()
                    : _context.Users.Where(x => memberIds.FirstOrDefault(id => id.UserId == x.Id) != null).ToList();

                var lastMessage = _context.Messages.FirstOrDefault(x => x.Id == groupContext.LastMessageId);
                var memInfo = new List<SimpleUser>();

                for (int i = 0; i < members.Count(); i++)
                {
                    memInfo.Add(ConvertToSimpleUser(members[i], userId));
                }

                group = new GroupInfo
                (
                    groupContext.ID,
                    groupContext.GroupName,
                    memInfo,
                    groupContext.Avatar,
                    lastMessage
                );

                group.TourId = tourId;
                group.OtherUserId = tourMemberIds.FirstOrDefault(x => x.UserId != userId)?.UserId ?? 0;

                group.Tour = new SimpleTour(
                    tourId,
                    tour.Name,
                    tour.StartDay,
                    tour.EndDay,
                    tourMemberIds.Count(),
                    memInfo.FirstOrDefault(x => x.Id == tour.CreateById),
                    new List<SimpleUser>(),
                    tour.Price,
                    tourInfo,
                    tourMemberIds.FirstOrDefault(x => x.UserId == userId)?.JoinAt,
                    tourMemberIds.FirstOrDefault(x => x.UserId == userId)?.AcceptedAt);

                group.OtherUser = memInfo.FirstOrDefault(x => x.Id != userId);

                isSuccess = true;
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return isSuccess;
        }

        public bool TryGetUserChat(int userId, int otherUserId, out GroupInfo group)
        {
            bool isSuccess;

            try
            {
                DbService.ConnectDb(out _context);

                var user = _context.Users.FirstOrDefault(x => x.Id == userId);
                var otherUser = _context.Users.FirstOrDefault(x => x.Id == otherUserId);

                var userGroupIds = _context.UserGroup.Where(x => x.UserId == userId).Select(x => x.GroupId).ToList();
                var otherUserGroupIds = _context.UserGroup.Where(x => x.UserId == otherUserId).Select(x => x.GroupId).ToList();
                var similarGroupIds = userGroupIds.Where(x => otherUserGroupIds.Contains(x)).ToList();

                var groupContext = _context.Groups.FirstOrDefault(g => similarGroupIds.Contains(g.ID) && g.TourId < 1);

                if (groupContext == null)
                {
                    groupContext = new Group
                    {
                        Avatar = string.Empty,
                        CreatorId = userId,
                        GroupName = string.Empty,
                        LastMessageAt = DateTime.MinValue,
                        LastMessageId = 0,
                        LastUpdate = DateTime.UtcNow,
                        TourId = 0,
                    };

                    _context.Groups.Add(groupContext);
                    _context.SaveChanges();

                    _context.UserGroup.Add(new UserGroup
                    {
                        GroupId = groupContext.ID,
                        LastSeenMessageId = 0,
                        UserId = userId
                    });
                    _context.UserGroup.Add(new UserGroup
                    {
                        GroupId = groupContext.ID,
                        LastSeenMessageId = 0,
                        UserId = otherUserId
                    });

                    _context.SaveChanges();
                }

                groupContext.Avatar = string.IsNullOrEmpty(groupContext.Avatar)
                    ? otherUser.Avatar
                    : groupContext.Avatar;

                groupContext.GroupName = string.IsNullOrEmpty(groupContext.GroupName)
                    ? otherUser.Name
                    : groupContext.GroupName;

                var memberIds = _context.UserGroup.Where(x => x.GroupId == groupContext.ID);
                var members = _context.Users.Where(x => memberIds.FirstOrDefault(id => id.UserId == x.Id) != null).ToList();

                var lastMessage = _context.Messages.FirstOrDefault(x => x.Id == groupContext.LastMessageId);
                var memInfo = new List<SimpleUser>();

                for (int i = 0; i < members.Count(); i++)
                {
                    memInfo.Add(ConvertToSimpleUser(members[i], userId));
                }

                group = new GroupInfo
                (
                    groupContext.ID,
                    groupContext.GroupName,
                    memInfo,
                    groupContext.Avatar,
                    lastMessage
                );

                isSuccess = true;
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return isSuccess;
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

        public bool GetAllMessagesOfGroup(int page, int pageSize, int userId, int groupId, out List<MessageInfo> messages,
            out Pagination pagination)
        {
            messages = new List<MessageInfo>();
            pagination = null;
            bool isSuccess;

            try
            {
                CoreHelper.ValidatePageSize(ref page, ref pageSize);

                DbService.ConnectDb(out _context);

                var messagesContext = (
                    _context.Messages.Where(m => m.GroupId == groupId)
                                     .OrderByDescending(m => m.CreateAt)
                    )?.AsEnumerable().ToList();

                ////var listGroup = _context.UserGroup

                var total = messagesContext.Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    // If pageSize = 0 => Get all
                    messagesContext = pageSize <= 0
                        ? messagesContext
                        : messagesContext
                            .Skip(skip)
                            .Take(pageSize)
                            .ToList();

                    var userGroup = _context.UserGroup.FirstOrDefault(x => x.GroupId == groupId && x.UserId == userId);
                    userGroup.LastSeenMessageId = messagesContext.First()?.Id ?? 0;
                    _context.SaveChanges();

                    var memberIds = _context.UserGroup.Where(x => x.GroupId == groupId);
                    var members = _context.Users.Where(x => memberIds.FirstOrDefault(id => id.UserId == x.Id) != null).ToList();
                    var memInfo = new List<SimpleUser>();

                    for (int i = 0; i < members.Count(); i++)
                    {
                        memInfo.Add(ConvertToSimpleUser(members[i], userId));
                    }

                    foreach (var message in messagesContext)
                    {
                        var msgInfo = new MessageInfo
                        {
                            Content = message.Content,
                            CreateAt = message.CreateAt,
                            CreateBy = message.CreateBy,
                            DeletedAt = message.DeletedAt,
                            GroupId = message.GroupId,
                            Id = message.Id,
                            Images = message.Images,
                            Sender = memInfo.FirstOrDefault(x => x.Id == message.CreateBy)
                        };

                        messages.Add(msgInfo);
                    }
                }
                else
                {
                    messages = new List<MessageInfo>();
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
        public bool CreateGroupChat(int userId, string groupName, List<int> members, string avatar, out Group group)
        {
            bool isSuccess;
            group = null;

            try
            {

                DbService.ConnectDb(out _context);

                group = new Group
                {
                    CreatorId = userId,
                    GroupName = groupName,
                    Avatar = avatar,
                    LastUpdate = DateTime.UtcNow,
                };

                if (members.Count > 1)
                {

                }

                group = _context.Groups.Add(group).Entity;
                _context.SaveChanges();
                var groupId = group.ID;

                // Filter existed members
                members = members.Where(userId => _context.Users.FirstOrDefault(x => x.Id == userId) != null).ToList();

                var listGroupUsers = members.Select(userId =>
                {
                    var user = _context.Users.FirstOrDefault(x => x.Id == userId);

                    if (user == null)
                    {
                        throw new ExceptionWithMessage($"User not found");
                    }

                    return new UserGroup
                    {
                        GroupId = groupId,
                        UserId = user.Id,
                    };
                }).Where(x => x != null);

                _context.UserGroup.AddRange(listGroupUsers);
                _context.SaveChanges();
                isSuccess = true;
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return isSuccess;
        }

        public bool AddUsersToGroup(int userId, int[] addUserIds, int groupId, List<int> members)
        {
            bool isSuccess;

            try
            {

                do
                {
                    DbService.ConnectDb(out _context);

                    var group = _context.Groups.FirstOrDefault(g => g.ID == groupId);

                    if (group == null)
                    {
                        throw new ExceptionWithMessage($"Group not found");
                    }

                    if (addUserIds.Any(ui => _context.Users.FirstOrDefault(u => u.Id == ui) == null))
                    {
                        throw new ExceptionWithMessage($"User not found");
                    }
                    // Filter existed members
                    members = members.Where(user => _context.Users.FirstOrDefault(x => x.Id == user) != null).ToList();

                    var listGroupUsers = new List<UserGroup>();

                    foreach (var addUserId in addUserIds)
                    {
                        var addMember = _context.Users.FirstOrDefault(x => x.Id == addUserId);

                        if (addMember == null)
                        {
                            continue;
                        }

                        var groupMember = _context.UserGroup.FirstOrDefault(x => x.GroupId == groupId && x.UserId == addMember.Id);

                        // User not in group
                        if (groupMember == null)
                        {
                            listGroupUsers.Add(new UserGroup
                            {
                                GroupId = groupId,
                                UserId = userId,
                            });
                        }
                    }

                    _context.UserGroup.AddRange(listGroupUsers);
                    _context.SaveChanges();
                    isSuccess = true;
                } while (false);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return isSuccess;
        }


        public bool RemoveUsersInGroup(int userId, int[] removeUserIds, int groupId, List<int> members)
        {
            bool isSuccess;

            try
            {

                do
                {
                    DbService.ConnectDb(out _context);

                    var group = _context.Groups.FirstOrDefault(g => g.ID == groupId);

                    if (group == null)
                    {
                        throw new ExceptionWithMessage($"Group not found");
                    }

                    if (removeUserIds.Any(ui => _context.Users.FirstOrDefault(u => u.Id == ui) == null))
                    {
                        throw new ExceptionWithMessage($"User not found");
                    }

                    // Filter existed members
                    members = members.Where(user => _context.Users.FirstOrDefault(x => x.Id == user) != null).ToList();

                    var removeGroupUsers = new List<UserGroup>();

                    foreach (var addUserId in removeUserIds)
                    {
                        var removeMember = _context.Users.FirstOrDefault(x => x.Id == addUserId);

                        if (removeMember == null)
                        {
                            continue;
                        }

                        var groupMember = _context.UserGroup.FirstOrDefault(x => x.GroupId == groupId && x.UserId.Equals(removeMember.Id));

                        if (groupMember != null)
                        {
                            removeGroupUsers.Add(groupMember);
                        }
                    }

                    _context.UserGroup.RemoveRange(removeGroupUsers);
                    _context.SaveChanges();
                    isSuccess = true;
                } while (false);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return isSuccess;
        }

        public bool SendMessage(int userId, Message message)
        {
            bool isSuccess;

            try
            {

                do
                {
                    DbService.ConnectDb(out _context);

                    var group = _context.Groups.FirstOrDefault(g => g.ID == message.GroupId);
                    if (group == null)
                    {
                        throw new ExceptionWithMessage($"Group not found");
                    }

                    var memberIds = new List<string>();

                    if (group.TourId > 0)
                    {
                        var members = _context.TourMembers.Where(t => t.TourId == group.TourId);

                        foreach (var mem in members)
                        {
                            var user = _context.Users.FirstOrDefault(u => u.Id == mem.UserId);
                            if (user != null)
                            {
                                memberIds.Add(user.Email);
                            }
                        }
                    }
                    else
                    {
                        var members = _context.UserGroup.Where(x => x.GroupId == message.GroupId).ToList();

                        foreach (var mem in members)
                        {
                            var user = _context.Users.FirstOrDefault(u => u.Id == mem.UserId);
                            if (user != null)
                            {
                                memberIds.Add(user.Email);
                            }
                        }
                    }

                    var chatMessageHandler = new ChatMessageHandler(_connectionManager);
                    _ = chatMessageHandler.SendMessageToUsersAsync(JObject.FromObject(message).ToString(), memberIds.ToArray());


                    message.GroupId = group.ID;
                    _context.Messages.Add(message);
                    _context.SaveChanges();

                    group.LastMessageId = message.Id;
                    group.LastMessageAt = DateTime.Now;
                    _context.SaveChanges();
                    isSuccess = true;
                } while (false);
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
    }
}