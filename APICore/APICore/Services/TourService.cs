using System.Collections.Generic;
using APICore.DBContext;
using APICore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog;
using System.Linq;
using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using static APICore.Helpers.ErrorList;

namespace APICore.Services
{
    public interface ITourService
    {
        ErrorCode TryGetTotalMember(int tourId, out int totalMember);

        ErrorCode TryGetAllTours(int tourInfoId, int page, int pageSize, out List<Tour> tours,
            out Pagination pagination);
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

        public ErrorCode TryGetTotalMember(int tourId, out int totalMember)
        {
            totalMember = 0;
            ErrorCode errorCode;

            try
            {
                DbService.ConnectDb(out _context);
                totalMember = _context.TourMembers.Count(t => t.TourId == tourId);
                errorCode = ErrorCode.Success;
                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return errorCode;
        }

        public ErrorCode TryGetAllTours(int tourInfoId, int page, int pageSize, out List<Tour> tours,
            out Pagination pagination)
        {
            tours = null;
            pagination = null;
            var errorCode = ErrorCode.Fail;

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

                    pagination = new Pagination(total, page, pageSize);
                    errorCode = ErrorCode.Success;
                }

                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return errorCode;
        }
        
        public ErrorCode TryAddTour(Tour tour)
        {
            var errorCode = ErrorCode.Fail;

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

            return errorCode;
        }
    }
}