using APICore;
using APICore.DBContext;
using APICore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using static APICore.Helpers.ErrorList;

namespace WebMvcPluginTour.Services
{
    public class TourInfoService : ITourInfoService
    {
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.LOGGER;
        private PostgreSQLContext _context;

        public TourInfoService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public ErrorCode TryGetToursByUserId(int userId, int page, int pageSize, out List<TourInfo> tourInfos, out Pagination pagination)
        {
            tourInfos = null;
            pagination = null;
            ErrorCode errorCode = ErrorCode.Fail;

            try
            {
                ConnectDB();
                var listtourInfos = _context.TourInfos.Where(a => a.CreateById == userId).ToList<TourInfo>();

                var total = listtourInfos.Select(p => p.Id).Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    tourInfos = listtourInfos.Select(u => u)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToList();

                    pagination = new Pagination(total, page, pageSize);
                    errorCode = ErrorCode.Success;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DisconnectDB();
            }

            return errorCode;
        }
        
        public ErrorCode TryGetTours(int page, int pageSize, out List<TourInfo> tourInfos, out Pagination pagination)
        {
            tourInfos = null;
            pagination = null;
            ErrorCode errorCode = ErrorCode.Fail;

            try
            {
                ConnectDB();
                var listtourInfos = _context.TourInfos.ToList<TourInfo>();

                var total = listtourInfos.Select(p => p.Id).Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    tourInfos = listtourInfos.Select(u => u)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToList();

                    pagination = new Pagination(total, page, pageSize);
                    errorCode = ErrorCode.Success;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DisconnectDB();
            }

            return errorCode;
        }

        public ErrorCode TryGetTourInfoById(int tourId, out TourInfo tourInfos)
        {
            tourInfos = null;
            ErrorCode errorCode = ErrorCode.Fail;

            try
            {
                ConnectDB();
                tourInfos = _context.TourInfos.Where(a => a.Id == tourId).FirstOrDefault();
                errorCode = ErrorCode.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DisconnectDB();
            }

            return errorCode;
        }

        public bool TryAddTourInfo(TourInfo tourInfo)
        {
            bool isSuccess = false;
            try
            {
                ConnectDB();
                _context.TourInfos.Add(tourInfo);
                _context.SaveChanges();
                isSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DisconnectDB();
            }

            return isSuccess;
        }

        public bool TryUpdateTourInfo(TourInfo tourInfo)
        {
            bool isSuccess = false;
            try
            {
                ConnectDB();
                _context.TourInfos.Update(tourInfo);
                _context.SaveChanges();
                isSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DisconnectDB();
            }

            return isSuccess;
        }

        public bool TryRemoveTourInfo(int tourInfoId)
        {
            bool isSuccess = false;
            try
            {
                ConnectDB();
                var tourInfo = _context.TourInfos.FirstOrDefault(u => u.Id == tourInfoId);
                if (tourInfo != null)
                {
                    _context.TourInfos.Remove(tourInfo);
                    _context.SaveChanges();
                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DisconnectDB();
            }

            return isSuccess;
        }

        public ErrorCode TryGetPlaceById(int placeId, out Place place)
        {
            place = null;
            ErrorCode errorCode = ErrorCode.Fail;

            try
            {
                ConnectDB();
                place = _context.Places.Where(a => a.Id == placeId).FirstOrDefault();
                errorCode = ErrorCode.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DisconnectDB();
            }

            return errorCode;
        }

        public ErrorCode TryGetUserById(int userId, out User user)
        {
            user = null;
            ErrorCode errorCode = ErrorCode.Fail;

            try
            {
                ConnectDB();
                user = _context.Users.Where(a => a.Id == userId).FirstOrDefault();
                errorCode = ErrorCode.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DisconnectDB();
            }

            return errorCode;
        }

        #region ConnectDB
        private void ConnectDB()
        {
            if (_context == null)
            {
                DbContextOptions<PostgreSQLContext> options = new DbContextOptions<PostgreSQLContext>();
                _context = new PostgreSQLContext(options);
            }
        }

        private void DisconnectDB()
        {
            if (_context != null)
            {
                _context.Dispose();
                _context = null;
            }
        }
        #endregion
    }
}
