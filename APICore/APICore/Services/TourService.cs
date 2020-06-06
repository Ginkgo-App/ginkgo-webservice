﻿using System.Collections.Generic;
using APICore.DBContext;
using Microsoft.Extensions.Options;
using NLog;
using System.Linq;
using APICore.Entities;
using APICore.Helpers;
using APICore.Models;

namespace APICore.Services
{
    public interface ITourService
    {
        bool TryGetTotalMember(int tourId, out int totalMember);

        bool TryGetAllTours(int tourInfoId, int page, int pageSize, out List<Tour> tours,
            out Pagination pagination);

        bool TryGetTour(int id, out Tour tour);
        bool TryAddTour(Tour tour);
        bool TryUpdateTour(Tour tour);
        bool TryDeleteTour(int tourId);
        bool TryAddService( int tourId ,IEnumerable<int> serviceIds);
    }

    public class TourService : ITourService
    {
        private PostgreSQLContext _context;
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.Logger;

        public TourService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public bool TryGetTotalMember(int tourId, out int totalMember)
        {
            totalMember = 0;

            try
            {
                DbService.ConnectDb(out _context);
                totalMember = _context.TourMembers.Count(t => t.TourId == tourId);
                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryGetAllTours(int tourInfoId, int page, int pageSize, out List<Tour> tours,
            out Pagination pagination)
        {
            tours = null;
            pagination = null;

            try
            {
                CoreHelper.ValidatePageSize(ref page, ref pageSize);

                DbService.ConnectDb(out _context);
                var listTours = _context.Tours.ToList();

                var total = listTours.Select(p => p.TourInfoId).Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    tours = listTours.Where(u => u.TourInfoId == tourInfoId)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToList();
                }
                else
                {
                    tours = new List<Tour>();
                }

                pagination = new Pagination(total, page, pageSize);
                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryGetTour(int id, out Tour tour)
        {
            try
            {
                DbService.ConnectDb(out _context);
                tour = _context.Tours.SingleOrDefault(t => t.Id == id);
                _context.SaveChanges();
                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryAddTour(Tour tour)
        {
            try
            {
                DbService.ConnectDb(out _context);
                _context.Tours.Add(tour);
                _context.SaveChanges();
                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryUpdateTour(Tour tour)
        {
            try
            {
                DbService.ConnectDb(out _context);
                _context.Tours.Update(tour);
                _context.SaveChanges();
                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryDeleteTour(int tourId)
        {
            try
            {
                DbService.ConnectDb(out _context);
                var tour = _context.Tours.SingleOrDefault(t => t.Id == tourId);
                
                if (tour == null)
                {
                    throw new ExceptionWithMessage(message: "Tour not found");
                }

                _context.Tours.Remove(tour);
                _context.SaveChanges();
                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }
        
        public bool TryAddService( int tourId ,IEnumerable<int> serviceIds)
        {
            try
            {
                DbService.ConnectDb(out _context);

                var serviceList = serviceIds.ToList();
                var isServiceExist = serviceList.All(serviceId => _context.Services.FirstOrDefault(s => s.Id == serviceId) != null);

                if (!isServiceExist)
                {
                    throw new ExceptionWithMessage("Service not found");
                }
                
                var tour = _context.Tours.SingleOrDefault(t => t.Id == tourId);
                
                if (tour == null)
                {
                    throw new ExceptionWithMessage(message: "Tour not found");
                }

                foreach (var serviceId in serviceList)
                {
                    _context.TourServices.Add(new Entities.TourService(serviceId, tour.Id));
                }
                 
                _context.SaveChanges();
                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }
    }
}