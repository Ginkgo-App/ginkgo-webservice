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
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.Logger;

        public TourService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public ErrorCode TryGetTotalMember(int tourId, out int totalMember)
        {
            totalMember = 0;
            ErrorCode errorCode;

            try
            {
                DbService.ConnectDb(ref _context);
                totalMember = _context.TourMembers.Count(t => t.TourId == tourId);
                errorCode = ErrorCode.Success;
                DbService.DisconnectDb(ref _context);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return errorCode;
        }
    }
}
