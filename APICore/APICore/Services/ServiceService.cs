using System.Collections.Generic;
using System.Linq;
using APICore.DBContext;
using APICore.Entities;
using APICore.Models;
using Microsoft.Extensions.Options;
using NLog;

namespace APICore.Services
{
    public interface IServiceService
    {
        bool TryGetServiceByTourId(int tourId, out List<Service> tourServices);
        bool TryGetServiceCreateByUser(int userId, out List<Service> tourServices);
        bool TryGetServiceById(int serviceId, out Service service);
        bool TryAddService(Service service, int createByUserId);
        bool TryUpdateService(Service service);
        bool TryDeleteService(int serviceId);
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

        public bool TryGetServiceByTourId(int tourId, out List<Service> tourServices)
        {
            var isSuccess = false;
            tourServices = new List<Service>();
            do
            {
                try
                {
                    DbService.ConnectDb(out _context);
                    var tourServiceIds = _context.TourServices.Where(s => s.TourId == tourId && s.DeletedAt == null)
                        .ToList();

                    if (!tourServiceIds.Any())
                    {
                        throw new ExceptionWithMessage("Tour not found");
                        break;
                    }

                    foreach (var tourServiceId in tourServiceIds)
                    {
                        var service = _context.Services.FirstOrDefault(s => s.Id.Equals(tourServiceId.ServiceId));
                        if (service == null)
                        {
                            continue;
                        }

                        tourServices.Add(service);
                    }

                    tourServices = tourServices.Distinct().ToList();
                    isSuccess = true;
                }
                finally
                {
                    DbService.DisconnectDb(out _context);
                }
            } while (false);

            return isSuccess;
        }

        public bool TryGetServiceCreateByUser(int userId, out List<Service> tourServices)
        {
            var isSuccess = false;
            tourServices = new List<Service>();
            do
            {
                try
                {
                    DbService.ConnectDb(out _context);

                    tourServices = (from serviceDetail in _context.ServiceDetails
                        where serviceDetail.CreateById == userId
                        join service in _context.Services
                            on serviceDetail.Id equals service.Id
                        select service)?.ToList();

                    isSuccess = true;
                }
                finally
                {
                    DbService.DisconnectDb(out _context);
                }
            } while (false);

            return isSuccess;
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

        public bool TryAddService(Service service, int createByUserId)
        {
            do
            {
                try
                {
                    DbService.ConnectDb(out _context);
                    _context.Services.Add(service);
                    _context.SaveChanges();
                }
                finally
                {
                    DbService.DisconnectDb(out _context);
                }
            } while (false);

            return true;
        }

        public bool TryUpdateService(Service service)
        {
            do
            {
                try
                {
                    DbService.ConnectDb(out _context);
                    _context.Services.Update(service);
                    _context.SaveChanges();
                }
                finally
                {
                    DbService.DisconnectDb(out _context);
                }
            } while (false);

            return true;
        }

        public bool TryDeleteService(int serviceId)
        {
            do
            {
                try
                {
                    DbService.ConnectDb(out _context);
                    var service = _context.Services.FirstOrDefault(s => s.Id == serviceId) ??
                                  throw new ExceptionWithMessage("Service not found");
                    service.Delete();
                    _context.SaveChanges();
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