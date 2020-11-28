using APICore.DBContext;
using APICore.Entities;
using APICore.Helpers;
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
        bool CreateGroupChat(int userId, string groupName, List<int> members, string avatar);
        bool GetAllGroupChat(int page, int pageSize, int userId, out List<GroupInfo> groups, out Pagination pagination);
    }

    public class ChatService : IChatService
    {
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.Logger;
        private PostgreSQLContext _context;
        private readonly FriendService _friendService;

        public ChatService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _friendService = new FriendService(appSettings);
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
                // var listPlaces =  _context.Places.Where(t => t.DeletedAt == null).ToList();

                var listGroup = (
                    from ug in _context.UserGroup
                    join g in _context.Groups
                        on ug.GroupId equals g.ID
                    where ug.ID == userId
                    group ug by ug.GroupId into gc
                    select new
                    {
                        Group = gc.Key,
                        Members = gc.ToList(),
                    })?.AsEnumerable().ToList();

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

        public bool CreateGroupChat(int userId, string groupName, List<int> members, string avatar)
        {
            bool isSuccess;

            try
            {

                DbService.ConnectDb(out _context);

                var group = new Group
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
                        ID = userId,
                        GroupId = group.ID,
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

        public bool RecieveMessage(int userId, int groupId)
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

                    var memberIds = _context.UserGroup.Where(x => x.GroupId == groupId);
                    var members = memberIds.Select(id => _context.Users.FirstOrDefault(u => u.Id == id.UserId))
                        .Where(x => x != null);

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