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

                DbService.ConnectDb(ref _context);
                var listTourInfos = _context.TourInfos.Where(a => a.CreateById == userId).ToList();

                var total = listTourInfos.Select(p => p.Id).Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    tourInfos = listTourInfos.Select(u => u)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToList();

                    pagination = new Pagination(total, page, pageSize);
                    errorCode = ErrorCode.Success;
                }
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
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

                DbService.ConnectDb(ref _context);
                var listTourInfos = _context.TourInfos.ToList();

                var total = listTourInfos.Select(p => p.Id).Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    tourInfos = listTourInfos.Select(u => u)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToList();

                    pagination = new Pagination(total, page, pageSize);
                    errorCode = ErrorCode.Success;
                }

                DbService.DisconnectDb(ref _context);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
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

                DbService.ConnectDb(ref _context);
                var listTours = _context.Tours.ToList();

                var total = listTours.Select(p => p.TourInfoId).Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    tourInfos = listTours.Where(u => u.TourInfoId == tourInfoId)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToList();

                    pagination = new Pagination(total, page, pageSize);
                    errorCode = ErrorCode.Success;
                }

                DbService.DisconnectDb(ref _context);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return errorCode;
        }

        public ErrorCode TryGetTourInfoById(int tourId, out TourInfo tourInfos)
        {
            tourInfos = null;
            ErrorCode errorCode;

            try
            {
                DbService.ConnectDb(ref _context);
                tourInfos = _context.TourInfos.FirstOrDefault(a => a.Id == tourId);
                errorCode = ErrorCode.Success;
                DbService.DisconnectDb(ref _context);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return errorCode;
        }

        public bool TryAddTourInfo(TourInfo tourInfo)
        {
            try
            {
                DbService.ConnectDb(ref _context);
                _context.TourInfos.Add(tourInfo);
                _context.SaveChanges();
                DbService.DisconnectDb(ref _context);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool TryUpdateTourInfo(TourInfo tourInfo)
        {
            try
            {
                DbService.ConnectDb(ref _context);
                _context.TourInfos.Update(tourInfo);
                _context.SaveChanges();
                DbService.DisconnectDb(ref _context);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool TryRemoveTourInfo(int tourInfoId)
        {
            bool isSuccess = false;
            try
            {
                DbService.ConnectDb(ref _context);
                var tourInfo = _context.TourInfos.FirstOrDefault(u => u.Id == tourInfoId);
                if (tourInfo != null)
                {
                    _context.TourInfos.Remove(tourInfo);
                    _context.SaveChanges();
                    isSuccess = true;
                }

                DbService.DisconnectDb(ref _context);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return isSuccess;
        }

        public ErrorCode TryGetPlaceById(int placeId, out Place place)
        {
            place = null;
            var errorCode = ErrorCode.Fail;

            try
            {
                DbService.ConnectDb(ref _context);
                place = _context.Places.FirstOrDefault(a => a.Id == placeId);
                errorCode = ErrorCode.Success;
                DbService.DisconnectDb(ref _context);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return errorCode;
        }

        public ErrorCode TryGetUserById(int userId, out User user)
        {
            user = null;
            var errorCode = ErrorCode.Fail;

            try
            {
                DbService.ConnectDb(ref _context);
                user = _context.Users.FirstOrDefault(a => a.Id == userId);
                errorCode = ErrorCode.Success;
                DbService.DisconnectDb(ref _context);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return errorCode;
        }

        public ErrorCode TryGetServiceById(int serviceId, out Service service)
        {
            service = null;
            var errorCode = ErrorCode.Fail;

            try
            {
                DbService.ConnectDb(ref _context);
                service = _context.Services.FirstOrDefault(a => a.Id == serviceId);
                errorCode = ErrorCode.Success;
                DbService.DisconnectDb(ref _context);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
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