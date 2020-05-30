using APICore.DBContext;
using APICore.Entities;
using APICore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog;
using System.Linq;
using static APICore.Helpers.ErrorList;

namespace APICore.Services
{
    public class FriendService : IFriendService
    {
        private PostgreSQLContext _context;
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.Logger;

        public FriendService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public ErrorCode CountTotalFriendAsync(int userId, out int total)
        {
            ErrorCode errorCode;
            total = 0;

            try
            {
                DbService.ConnectDb(out _context);
                total = _context.Friends.CountAsync(u => u.UserId == userId).Result;
                errorCode = ErrorCode.Success;
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return errorCode;
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

            if (friendDb.IsAccepted)
            {
                return FriendType.Accepted;
            }

            if (userId == friendDb.UserId)
            {
                return FriendType.Waiting;
            }

            return userId == friendDb.RequestedUserId ? FriendType.Requested : FriendType.None;
        }

        public ErrorCode TryAddFriend(int userId, int userRequestId)
        {
            ErrorCode errorCode;

            do
            {
                TryGetFriendRequest(userId, userRequestId, out var friendDb);
                if (friendDb != null)
                {
                    if (!friendDb.IsAccepted)
                    {
                        errorCode = ErrorCode.FriendRequestAlreadySent;
                        break;
                    }

                    errorCode = ErrorCode.AlreadyFriend;
                    break;
                }

                var friend = new Friend(userId, userRequestId);

                try
                {
                    DbService.ConnectDb(out _context);
                    _context.Friends.Add(friend);
                    _context.SaveChanges();
                }
                finally
                {
                    DbService.DisconnectDb(out _context);
                }

                errorCode = ErrorCode.Success;
            } while (false);

            return errorCode;
        }


        public ErrorCode TryAcceptFriend(int userId, int userRequestId)
        {
            ErrorCode errorCode;

            do
            {
                try
                {
                    DbService.ConnectDb(out _context);
                    var friendDBs = _context.Friends.Where(f =>
                            f.UserId == userId && f.RequestedUserId == userRequestId)
                        .ToArray();

                    var friendDb = friendDBs.Length > 0 ? friendDBs[0] : null;
                    if (friendDb == null)
                    {
                        errorCode = ErrorCode.FriendRequestNotFound;
                        break;
                    }

                    if (friendDb.IsAccepted)
                    {
                        errorCode = ErrorCode.AlreadyFriend;
                        break;
                    }

                    friendDb.IsAccepted = true;
                    _context.Friends.Update(friendDb);
                    _context.SaveChanges();
                }
                finally
                {
                    DbService.DisconnectDb(out _context);
                }

                errorCode = ErrorCode.Success;
            } while (false);

            return errorCode;
        }

        public ErrorCode TryRemoveFriend(int userId, int userRequestId)
        {
            ErrorCode errorCode;
            do
            {
                TryGetFriendRequest(userId, userRequestId, out var friendDb);
                if (friendDb != null)
                {
                    try
                    {
                        DbService.ConnectDb(out _context);
                        _context.Friends.Remove(friendDb);
                        _context.SaveChanges();

                        errorCode = ErrorCode.Success;
                    }
                    finally
                    {
                        DbService.DisconnectDb(out _context);
                    }
                }
                else
                {
                    errorCode = ErrorCode.FriendNotFound;
                }
            } while (false);

            return errorCode;
        }

        private void TryGetFriendRequest(int userId, int userOtherId, out Friend friendDb)
        {
            try
            {
                DbService.ConnectDb(out _context);
                var friendDBs = _context.Friends.Where(a =>
                        (a.UserId == userId && a.RequestedUserId == userOtherId)
                        || (a.RequestedUserId == userId && a.UserId == userOtherId))
                    .ToArray();

                friendDb = friendDBs.Length > 0 ? friendDBs[0] : null;
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }
        }
    }
}