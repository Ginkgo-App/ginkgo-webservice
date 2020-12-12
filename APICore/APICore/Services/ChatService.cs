using APICore.DBContext;
using APICore.Entities;
using APICore.Helpers;
using APICore.Middlewares;
using APICore.Models;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace APICore.Services
{
    public interface IChatService
    {
        bool AddUsersToGroup(int userId, int[] addUserIds, int groupId, List<int> members);
        bool CreateGroupChat(int userId, string groupName, List<int> members, string avatar, out Group group);
        bool GetAllGroupChat(int page, int pageSize, int userId, out List<GroupInfo> groups, out Pagination pagination);
        bool RemoveUsersInGroup(int userId, int[] removeUserIds, int groupId, List<int> members);
        bool SendMessage(int userId, Message message);
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

            try
            {
                CoreHelper.ValidatePageSize(ref page, ref pageSize);

                DbService.ConnectDb(out _context);

                var listGroup = (
                    from ug in _context.UserGroup
                    join g in _context.Groups
                        on ug.GroupId equals g.ID
                    where ug.ID == userId
                    group ug by ug.GroupId into gc
                    select new
                    {
                        Group = gc.Key,
                        Members = gc.Count(),
                    })?.AsEnumerable().ToList();

                ////var listGroup = _context.UserGroup

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

                    foreach (var group in listGroup)
                    {

                        //groups.Add(new ListGroupInfo(group.Group, "Test", group.Members.Select(x=>x.UserName));
                    }
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

                    var members = _context.UserGroup.Where(x => x.GroupId == message.GroupId).ToList();
                    var memberIds = new List<string>();
                    foreach (var mem in members)
                    {
                        var user = _context.Users.FirstOrDefault(u => u.Id == mem.UserId);
                        if (user != null)
                        {
                            memberIds.Add(user.Email);
                        }
                    }

                    var chatMessageHandler = new ChatMessageHandler(_connectionManager);
                    _ = chatMessageHandler.SendMessageToUsersAsync(message.ToString(), memberIds.ToArray());

                    isSuccess = true;
                } while (false);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return isSuccess;
        }
    }
}