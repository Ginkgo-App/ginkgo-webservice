using APICore.DBContext;
using Microsoft.EntityFrameworkCore;
using NLog;
using Npgsql;
using System;
using System.Linq;
using static APICore.Helpers.ErrorList;

namespace APICore.Services
{
    public class FriendService
    {
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.LOGGER;
        private PostgreSQLContext _context;

        public FriendService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public ErrorCode CountTotalFriend(int userId, out int total)
        {
            ErrorCode errorCode = ErrorCode.Default;
            total = 0;

            try
            {
                ConnectDB();
                total = _context.Friends.Where(u => u.UserId == userId).Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DisconnectDB();
            }

            return errorCode;
        }

        #region ConnectDB
        private void ConnectDB()
        {
            if (_context == null)
            {
                DbContextOptions<PostgreSQLContext> options = new DbContextOptions<PostgreSQLContext>();
                _context = new PostgreSQLContext(options);
            }
        }

        private void DisconnectDB()
        {
            if (_context != null)
            {
                _context.Dispose();
                _context = null;
            }
        }
        #endregion
    }
}
