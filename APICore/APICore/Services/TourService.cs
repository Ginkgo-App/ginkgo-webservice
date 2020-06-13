using System.Collections.Generic;
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
        bool TryGetTimelines(int tourId, out List<TimeLine> timelines);
        bool TryAddTour(Tour tour, List<TimeLine> timeLines);
        bool TryUpdateTour(Tour tour);
        bool TryDeleteTour(int tourId);
        bool TryAddService(int tourId, IEnumerable<int> serviceIds);
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
                var listTours = _context.Tours.Where(t => t.DeletedAt == null && t.TourInfoId == tourInfoId)?.ToList();

                var total = listTours?.Count() ?? 0;
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    tours = pageSize <= 0
                        ? listTours
                        : listTours
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
                tour = _context.Tours.SingleOrDefault(t => t.Id == id) ?? throw  new ExceptionWithMessage("Tour not found");
                
                TryGetTourInfo(id, out var tourInfo);
                TryGetTimelines(id, out var timeLines);
                
                tour.TimeLines = timeLines;
                tour.TourInfo = tourInfo;
                _context.SaveChanges();
                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }
        
        public bool TryGetTourInfo(int tourId, out TourInfo tourInfo)
        {
            try
            {
                DbService.ConnectDb(out _context);
                var tour = _context.Tours.SingleOrDefault(t => t.Id == tourId) ?? throw new ExceptionWithMessage("Tour not found");
                tourInfo = _context.TourInfos.FirstOrDefault(t => t.Id == tour.TourInfoId);

                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }
        
        public bool TryGetTimelines(int tourId, out List<TimeLine> timelines)
        {
            try
            {
                DbService.ConnectDb(out _context);
                var tour = _context.Tours.SingleOrDefault(t => t.Id == tourId) ?? throw new ExceptionWithMessage("Tour not found");

                timelines = _context.TimeLines.Where(t => t.TourId == tourId)?.ToList() ?? new List<TimeLine>();

                foreach (var timeLine in timelines)
                {
                    var timelineDetails = _context.TimelineDetails.Where(td => td.TimelineId == timeLine.Id);
                    
                    
                    timeLine.TimelineDetails ??= new List<TimelineDetail>();
                    timeLine.TimelineDetails.AddRange(timelineDetails);
                }
                
                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryAddTour(Tour tour, List<TimeLine> timeLines)
        {
            try
            {                
                DbService.ConnectDb(out _context);
                
                // Store tour
                _context.Tours.Add(tour);
                _context.SaveChanges();
                
                // Store Timelines
                foreach (var timeline in timeLines)
                {
                    timeline.TourId = tour.Id;
                    _context.TimeLines.Add(timeline);
                    _context.SaveChanges();

                    if (timeline.TimelineDetails != null)
                        foreach (var timelineDetail in timeline.TimelineDetails)
                        {
                            var place = _context.Places.FirstOrDefault(p => p.Id == timelineDetail.PlaceId);

                            // Check place is exist
                            if (place == null)
                            {
                                continue;
                            }

                            // Store time line detail
                            timelineDetail.TimelineId = timeline.Id;
                            _context.TimelineDetails.Add(timelineDetail);
                            _context.SaveChanges();
                        }
                }
                
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

        public bool TryAddService(int tourId, IEnumerable<int> serviceIds)
        {
            try
            {
                DbService.ConnectDb(out _context);

                var serviceList = serviceIds.ToList();
                var isServiceExist = serviceList.All(serviceId =>
                    _context.Services.FirstOrDefault(s => s.Id == serviceId) != null);

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