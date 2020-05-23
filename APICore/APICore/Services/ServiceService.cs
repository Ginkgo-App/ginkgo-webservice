using System.Collections.Generic;
using System.Linq;
using APICore.DBContext;
using APICore.Entities;
using APICore.Helpers;
using APICore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog;

namespace APICore.Services
{
    public class ServiceService : IServiceService
    {
        private PostgreSQLContext _context;
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.Logger;

        public ServiceService(IOptions<AppSettings> appSettings, PostgreSQLContext context)
        {
            _appSettings = appSettings.Value;
            _context = context;
        }
        
        public ErrorList.ErrorCode TryGetServiceByTourId(int tourId, out List<APICore.Entities.TourService> tourServices)
        {
            ErrorList.ErrorCode errorCode;
            do
            {
                    try
                    {
                        DbService.ConnectDb(ref _context);
                        tourServices = _context.TourServices.Where(s => s.TourId == tourId).ToList();

                        if (!tourServices.Any())
                        {
                            errorCode = ErrorList.ErrorCode.ServiceNotFound;
                            break;
                        }
                        errorCode = ErrorList.ErrorCode.Success;
                    }
                    finally
                    {
                        DbService.DisconnectDb(ref _context);
                    }
            } while (false);

            return errorCode;
        }
    }
}