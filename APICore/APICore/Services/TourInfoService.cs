using APICore.DBContext;
using APICore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using APICore.Models;
using static APICore.Helpers.CoreHelper;
using static APICore.Helpers.ErrorList;

namespace APICore.Services
{
    public interface ITourInfoService
    {
        ErrorCode TryGetToursByUserId(int userId, int page, int pageSize, out List<TourInfo> tourInfos,
            out Pagination pagination);

        ErrorCode TryGetTourInfos(int page, int pageSize, out List<TourInfo> tourInfos,
            out Pagination pagination);

        ErrorCode TryGetTours(int tourInfoId, int page, int pageSize, out List<Tour> tourInfos,
            out Pagination pagination);

        ErrorCode TryGetTourInfoById(int tourId, out TourInfo tourInfos);
        bool TryAddTourInfo(TourInfo tourInfo);
        bool TryUpdateTourInfo(TourInfo tourInfo);
        bool TryRemoveTourInfo(int tourInfoId);
        ErrorCode TryGetPlaceById(int placeId, out Place place);
        ErrorCode TryGetUserById(int userId, out User user);
        ErrorCode TryGetServiceById(int serviceId, out Service service);
    }

    public class TourInfoService : ITourInfoService
    {
        private PostgreSQLContext _context;
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.Logger;

        public TourInfoService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public ErrorCode TryGetToursByUserId(int userId, int page, int pageSize, out List<TourInfo> tourInfos,
            out Pagination pagination)
        {
            tourInfos = null;
            pagination = null;
            var errorCode = ErrorCode.Fail;

            try
            {
                ValidatePageSize(ref page, ref pageSize);

                DbService.ConnectDb(out _context);
                var listTourInfos = _context.TourInfos.Where(a => a.CreateById == userId && a.DeletedAt == null)
                    .ToList();

                var total = listTourInfos.Select(p => p.Id).Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    // If pageSize <= 0 then get all tour info
                    tourInfos = pageSize <= 0
                        ? listTourInfos
                        : listTourInfos.Select(u => u)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToList();
                }
                else
                {
                    tourInfos = new List<TourInfo>();
                }

                pagination = new Pagination(total, page, pageSize > 0 ? pageSize : total);
                errorCode = ErrorCode.Success;
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return errorCode;
        }

        public ErrorCode TryGetTourInfos(int page, int pageSize, out List<TourInfo> tourInfos,
            out Pagination pagination)
        {
            tourInfos = null;
            pagination = null;
            var errorCode = ErrorCode.Fail;

            try
            {
                ValidatePageSize(ref page, ref pageSize);

                DbService.ConnectDb(out _context);
                var listTourInfos = _context.TourInfos.Where(t => t.DeletedAt == null).ToList();

                var total = listTourInfos.Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    tourInfos =
                        pageSize <= 0
                            ? listTourInfos
                            : listTourInfos.Select(u => u)
                                .Skip(skip)
                                .Take(pageSize)
                                .ToList();
                }
                else
                {
                    tourInfos = new List<TourInfo>();
                }

                pagination = new Pagination(total, page, pageSize > 0 ? pageSize : total);
                errorCode = ErrorCode.Success;

                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return errorCode;
        }

        public ErrorCode TryGetTours(int tourInfoId, int page, int pageSize, out List<Tour> tourInfos,
            out Pagination pagination)
        {
            tourInfos = null;
            pagination = null;
            ErrorCode errorCode = ErrorCode.Fail;

            try
            {
                ValidatePageSize(ref page, ref pageSize);

                DbService.ConnectDb(out _context);
                var listTours = _context.Tours.Where(t => t.DeletedAt == null).ToList();

                var total = listTours.Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    tourInfos = pageSize <= 0
                        ? listTours
                        : listTours.Where(u => u.TourInfoId == tourInfoId)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToList();
                }
                else
                {
                    tourInfos = new List<Tour>();
                }

                pagination = new Pagination(total, page, pageSize > 0 ? pageSize : total);
                errorCode = ErrorCode.Success;

                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return errorCode;
        }

        public ErrorCode TryGetTourInfoById(int tourId, out TourInfo tourInfos)
        {
            tourInfos = null;
            ErrorCode errorCode;

            try
            {
                DbService.ConnectDb(out _context);
                tourInfos = _context.TourInfos.FirstOrDefault(a => a.Id == tourId);
                errorCode = ErrorCode.Success;
                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return errorCode;
        }

        public bool TryAddTourInfo(TourInfo tourInfo)
        {
            try
            {
                DbService.ConnectDb(out _context);
                _context.TourInfos.Add(tourInfo);
                _context.SaveChanges();
                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryUpdateTourInfo(TourInfo tourInfo)
        {
            try
            {
                DbService.ConnectDb(out _context);
                _context.TourInfos.Update(tourInfo);
                _context.SaveChanges();
                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryRemoveTourInfo(int tourInfoId)
        {
            bool isSuccess = false;
            try
            {
                DbService.ConnectDb(out _context);
                var tourInfo = _context.TourInfos.FirstOrDefault(u => u.Id == tourInfoId);
                if (tourInfo != null)
                {
                    tourInfo.Delete();
                    _context.SaveChanges();
                    isSuccess = true;
                }

                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return isSuccess;
        }

        public ErrorCode TryGetPlaceById(int placeId, out Place place)
        {
            place = null;
            var errorCode = ErrorCode.Fail;

            try
            {
                DbService.ConnectDb(out _context);
                place = _context.Places.FirstOrDefault(a => a.Id == placeId);
                errorCode = ErrorCode.Success;
                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return errorCode;
        }

        public ErrorCode TryGetUserById(int userId, out User user)
        {
            user = null;
            var errorCode = ErrorCode.Fail;

            try
            {
                DbService.ConnectDb(out _context);
                user = _context.Users.FirstOrDefault(a => a.Id == userId);
                errorCode = ErrorCode.Success;
                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return errorCode;
        }

        public ErrorCode TryGetServiceById(int serviceId, out Service service)
        {
            service = null;
            var errorCode = ErrorCode.Fail;

            try
            {
                DbService.ConnectDb(out _context);
                service = _context.Services.FirstOrDefault(a => a.Id == serviceId);
                errorCode = ErrorCode.Success;
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