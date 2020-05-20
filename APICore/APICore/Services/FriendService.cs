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

        public ErrorCode CountTotalFriend(int userId, out int total)
        {
            ErrorCode errorCode;
            total = 0;

            try
            {
                ConnectDb();
                total = _context.Friends.Count(u => u.UserId == userId);
                errorCode = ErrorCode.Success;
            }
            finally
            {
                DisconnectDb();
            }

            return errorCode;
        }

        public int CalculateIsFriend(int userId, int userRequestId)
        {
            TryGetFriendRequest(userId, userRequestId, out var friendDb);
            if (friendDb == null)
            {
                return 0;
            }

            if (friendDb.IsAccepted)
            {
                return 3;
            }

            if (userId == friendDb.UserId)
            {
                return 1;
            }

            if (userId == friendDb.RequestedUserId)
            {
                return 2;
            }

            return -1;
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

                var friend = new Friend
                {
                    UserId = userId,
                    RequestedUserId = userRequestId,
                    IsAccepted = false,
                };

                try
                {
                    ConnectDb();
                    _context.Friends.Add(friend);
                    _context.SaveChanges();
                }
                finally
                {
                    DisconnectDb();
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
                    ConnectDb();
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
                    DisconnectDb();
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
                        ConnectDb();
                        _context.Friends.Remove(friendDb);
                        _context.SaveChanges();

                        errorCode = ErrorCode.Success;
                    }
                    finally
                    {
                        DisconnectDb();
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
                ConnectDb();
                var friendDBs = _context.Friends.Where(a =>
                        (a.UserId == userId && a.RequestedUserId == userOtherId) ||
                        (a.RequestedUserId == userId && a.UserId == userOtherId))
                    .ToArray();

                friendDb = friendDBs.Length > 0 ? friendDBs[0] : null;
            }
            finally
            {
                DisconnectDb();
            }
        }

        #region ConnectDB

        private void ConnectDb()
        {
            if (_context != null) return;
            var options = new DbContextOptions<PostgreSQLContext>();
            _context = new PostgreSQLContext(options);
        }

        private void DisconnectDb()
        {
            if (_context == null) return;
            _context.Dispose();
            _context = null;
        }

        #endregion
    }
}