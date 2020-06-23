﻿using APICore.DBContext;
using APICore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using APICore.Models;
using static APICore.Helpers.CoreHelper;
using static APICore.Helpers.ErrorList;

namespace APICore.Services
{
    public interface ITourInfoService
    {
        ErrorCode TryGetTourInfosByUserId(int userId, int page, int pageSize, out List<TourInfo> tourInfos,
            out Pagination pagination);

        ErrorCode TryGetTourInfos(int page, int pageSize, out List<TourInfo> tourInfos,
            out Pagination pagination);

        ErrorCode TryGetTours(int tourInfoId, int page, int pageSize, out List<Tour> tours,
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

        public ErrorCode TryGetTourInfosByUserId(int userId, int page, int pageSize, out List<TourInfo> tourInfos,
            out Pagination pagination)
        {
            tourInfos = null;
            pagination = null;
            ErrorCode errorCode;

            try
            {
                ValidatePageSize(ref page, ref pageSize);

                DbService.ConnectDb(out _context);
                var listTourInfos = _context.TourInfos.Where(a => a.CreateById == userId && a.DeletedAt == null)
                    .OrderByDescending(a=>a.Id)
                    .ToList();

                var total = listTourInfos.Select(p => p.Id).Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    // If pageSize <= 0 then get all tour info
                    tourInfos = pageSize <= 0
                        ? listTourInfos
                        : listTourInfos
                            .Skip(skip)
                            .Take(pageSize)
                            .ToList();
                }
                else
                {
                    tourInfos = new List<TourInfo>();
                }

                foreach (var tourInfo in tourInfos)
                {
                    TryGetPlaceById(tourInfo.StartPlaceId, out var startPlace);
                    TryGetPlaceById(tourInfo.DestinatePlaceId, out var destinatePlace);

                    tourInfo.StartPlace = startPlace;
                    tourInfo.DestinatePlace = destinatePlace;
                }

                pagination = new Pagination(total, page, pageSize > 0 ? pageSize : total);
                errorCode = ErrorCode.Success;
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
            ErrorCode errorCode;

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
                            : listTourInfos
                                .Skip(skip)
                                .Take(pageSize)
                                .ToList();
                }
                else
                {
                    tourInfos = new List<TourInfo>();
                }

                foreach (var tourInfo in tourInfos)
                {
                    TryGetPlaceById(tourInfo.StartPlaceId, out var startPlace);
                    TryGetPlaceById(tourInfo.DestinatePlaceId, out var destinatePlace);

                    tourInfo.StartPlace = startPlace;
                    tourInfo.DestinatePlace = destinatePlace;
                }

                tourInfos = tourInfos.OrderByDescending(t => t.Id).ToList();

                pagination = new Pagination(total, page, pageSize > 0 ? pageSize : total);
                errorCode = ErrorCode.Success;
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return errorCode;
        }

        public ErrorCode TryGetTours(int tourInfoId, int page, int pageSize, out List<Tour> tours,
            out Pagination pagination)
        {
            tours = null;
            pagination = null;
            ErrorCode errorCode;

            try
            {
                ValidatePageSize(ref page, ref pageSize);

                DbService.ConnectDb(out _context);
                var listTours = _context.Tours.Where(t => t.DeletedAt == null && t.TourInfoId == tourInfoId).ToList();

                var total = listTours.Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    tours = pageSize <= 0
                        ? listTours
                        : listTours.Where(u => u.TourInfoId == tourInfoId)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToList();
                }
                else
                {
                    tours = new List<Tour>();
                }

                foreach (var tour in tours)
                {
                    TryGetTimelines(tour.Id, out var timeLines);
                    TryGetTourInfoById(tour.TourInfoId, out var tourInfo);
                    tour.TimeLines = timeLines;
                    tour.TourInfo = tourInfo;
                }

                pagination = new Pagination(total, page, pageSize > 0 ? pageSize : total);
                errorCode = ErrorCode.Success;
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return errorCode;
        }

        public bool TryGetTimelines(int tourId, out List<TimeLine> timelines)
        {
            try
            {
                DbService.ConnectDb(out _context);
                var tour = _context.Tours.SingleOrDefault(t => t.Id == tourId && t.DeletedAt == null) ??
                           throw new ExceptionWithMessage("Tour not found");

                timelines = _context.TimeLines.Where(t => t.TourId == tourId && t.DeletedAt == null)?.ToList() ??
                            new List<TimeLine>();

                foreach (var timeLine in timelines)
                {
                    var timelineDetails =
                        _context.TimelineDetails.Where(td => td.TimelineId == timeLine.Id && td.DeletedAt == null);

                    timeLine.TimelineDetails ??= new List<TimelineDetail>();

                    timeLine.TimelineDetails.AddRange(timelineDetails);
                }

                _context.SaveChanges();
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
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
                DbService.ConnectDb(out _context);

                var destinatePlace = _context.Places.FirstOrDefault(p => p.Id == tourInfo.DestinatePlaceId) ??
                                     throw new ExceptionWithMessage("Place not found");
                var startPlace = _context.Places.FirstOrDefault(p => p.Id == tourInfo.StartPlaceId) ??
                                 throw new ExceptionWithMessage("Place not found");

                _context.TourInfos.Add(tourInfo);
                _context.SaveChanges();
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
                DbService.ConnectDb(out _context);
                _context.TourInfos.Update(tourInfo);
                _context.SaveChanges();
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
                DbService.ConnectDb(out _context);
                var tourInfo = _context.TourInfos.FirstOrDefault(u => u.Id == tourInfoId);
                if (tourInfo != null)
                {
                    tourInfo.Delete();
                    _context.SaveChanges();
                    isSuccess = true;
                }
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
            ErrorCode errorCode;

            try
            {
                DbService.ConnectDb(out _context);
                place = _context.Places.FirstOrDefault(a => a.Id == placeId) ??
                        throw new ExceptionWithMessage("Place not found");
                errorCode = ErrorCode.Success;
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
            ErrorCode errorCode;

            try
            {
                DbService.ConnectDb(out _context);
                user = _context.Users.FirstOrDefault(a => a.Id == userId);
                errorCode = ErrorCode.Success;
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
            ErrorCode errorCode;

            try
            {
                DbService.ConnectDb(out _context);
                service = _context.Services.FirstOrDefault(a => a.Id == serviceId);
                errorCode = ErrorCode.Success;
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return errorCode;
        }
    }
}