using System.Collections.Generic;
using System.Linq;
using APICore.DBContext;
using APICore.Entities;
using APICore.Helpers;
using Microsoft.Extensions.Options;
using NLog;

namespace APICore.Services
{
    public interface IServiceService
    {
        ErrorList.ErrorCode TryGetServiceByTourId(int tourId, out List<Service> tourServices);
        bool TryGetServiceById(int serviceId, out Service service);
    }

    public class ServiceService : IServiceService
    {
        private PostgreSQLContext _context;
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.Logger;

        public ServiceService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }
        
        public ErrorList.ErrorCode TryGetServiceByTourId(int tourId, out List<Service> tourServices)
        {
            ErrorList.ErrorCode errorCode;
            tourServices = new List<Service>();
            do
            {
                    try
                    {
                        DbService.ConnectDb(out _context);
                        var tourServiceIds = _context.TourServices.Where(s => s.TourId == tourId && s.DeletedAt == null).ToList();

                        if (!tourServiceIds.Any())
                        {
                            errorCode = ErrorList.ErrorCode.ServiceNotFound;
                            break;
                        }

                        foreach (var tourServiceId in tourServiceIds)
                        {
                            var service =  _context.Services.FirstOrDefault(s=>s.Id.Equals( tourServiceId.ServiceId));
                            if (service == null)
                            {
                                continue;
                            }
                            tourServices.Add(service);
                        }

                        tourServices = tourServices.Distinct().ToList();
                        errorCode = ErrorList.ErrorCode.Success;
                    }
                    finally
                    {
                        DbService.DisconnectDb(out _context);
                    }
            } while (false);

            return errorCode;
        }
        
        public bool TryGetServiceById(int serviceId, out Service service)
        {
            do
            {
                try
                {
                    DbService.ConnectDb(out _context);
                    service = _context.Services.FirstOrDefault(s => s.Id == serviceId);
                }
                finally
                {
                    DbService.DisconnectDb(out _context);
                }
            } while (false);

            return true;
        }
    }
}