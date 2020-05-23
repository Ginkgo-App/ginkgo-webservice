using APICore.DBContext;
using APICore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog;
using System.Linq;
using static APICore.Helpers.ErrorList;

namespace APICore.Services
{
    public class TourService : ITourService
    {
        private PostgreSQLContext _context;

        public ErrorCode TryGetTotalMember(int tourId, out int totalMember)
        {
            totalMember = 0;
            ErrorCode errorCode;

            try
            {
                ConnectDb();
                totalMember = _context.TourMembers.Count(t => t.TourId == tourId);
                errorCode = ErrorCode.Success;
                DisconnectDb();
            }
            finally
            {
                DisconnectDb();
            }

            return errorCode;
        }

        #region ConnectDB

        private void ConnectDb()
        {
            if (_context != null) return;
            _context = PostgreSQLContext.Instance;
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
