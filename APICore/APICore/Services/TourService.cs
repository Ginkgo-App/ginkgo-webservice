using System;
using System.Collections.Generic;
using APICore.DBContext;
using Microsoft.Extensions.Options;
using NLog;
using System.Linq;
using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using Microsoft.EntityFrameworkCore.Internal;

namespace APICore.Services
{
    public interface ITourService
    {
        bool TryGetTotalMember(int tourId, out int totalMember);

        bool TryGetAllTours(int tourInfoId, int page, int pageSize, out List<Tour> tours,
            out Pagination pagination);

        bool TryGetTourAllMembers(int myUserId, int tourId, int page, int pageSize, out List<SimpleTourMember> users,
            out Pagination pagination);

        bool TryGetTourRequestedMembers(int myUserId, int tourId, int page, int pageSize,
            out List<SimpleTourMember> users, out Pagination pagination);

        bool TryGetTourAcceptedMembers(int myUserId, int tourId, int page, int pageSize,
            out List<SimpleTourMember> users, out Pagination pagination);

        bool TryGetTour(int userId, int id, out Tour tour);
        bool TryGetTourInfo(int userId, int tourId, out TourInfo tourInfo);
        bool TryGetTimelines(int tourId, out List<TimeLine> timelines);
        bool TryAddTour(Tour tour, List<TimeLine> timeLines);
        bool TryJoinTour(Tour tour, User user);
        bool TryAcceptJoinTour(Tour tour, User user);
        bool TryUpdateTour(Tour tour);
        bool TryDeleteTour(int tourId);
        bool TryAddService(int tourId, IEnumerable<int> serviceIds);

        bool GetTopUser(int myUserId, int page, int pageSize, out List<SimpleUser> result,
            out Pagination pagination);

        bool GetTourListRecommend(int userId, int page, int pageSize, out List<SimpleTour> tours,
            out Pagination pagination);

        bool GetTourListForYou(int userId, int page, int pageSize, out List<SimpleTour> tours,
            out Pagination pagination);

        bool GetTourListFriend(int userId, int page, int pageSize, out List<SimpleTour> tours,
            out Pagination pagination);
    }

    public class TourService : ITourService
    {
        private PostgreSQLContext _context;
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.Logger;
        private readonly FriendService _friendService;

        public TourService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _friendService = new FriendService(appSettings);
        }

        public bool TryGetTotalMember(int tourId, out int totalMember)
        {
            totalMember = 0;

            try
            {
                DbService.ConnectDb(out _context);
                totalMember = _context.TourMembers.Count(t => t.TourId == tourId);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
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
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool TryGetTourAllMembers(int myUserId, int tourId, int page, int pageSize,
            out List<SimpleTourMember> users, out Pagination pagination)
        {
            try
            {
                DbService.ConnectDb(out _context);
                var members = (from tm in _context.TourMembers
                    join u in _context.Users on tm.UserId equals u.Id
                    where (tm.TourId == tourId && tm.DeletedAt == null)
                    select new
                    {
                        User = u,
                        TourMember = tm,
                    });

                var total = members?.Count() ?? 0;
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    members = pageSize <= 0
                        ? members
                        : members
                            .Skip(skip)
                            .Take(pageSize);

                    users = members.AsEnumerable().Select(e =>
                    {
                        var friendType = _friendService.CalculateIsFriend(myUserId, e.User.Id);

                        return new SimpleTourMember(
                            e.User.Id,
                            e.User.Name,
                            e.User.Avatar,
                            e.User.Job,
                            friendType,
                            e.TourMember.JoinAt,
                            e.TourMember.AcceptedAt
                        );
                    }).ToList();
                }
                else
                {
                    users = new List<SimpleTourMember>();
                }

                pagination = new Pagination(total, page, pageSize);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool TryGetTourRequestedMembers(int myUserId, int tourId, int page, int pageSize,
            out List<SimpleTourMember> users, out Pagination pagination)
        {
            try
            {
                DbService.ConnectDb(out _context);
                var members = (from tm in _context.TourMembers
                    join u in _context.Users on tm.UserId equals u.Id
                    where (tm.TourId == tourId && tm.DeletedAt == null && tm.AcceptedAt == null)
                    select new
                    {
                        User = u,
                        TourMember = tm,
                    });

                var total = members?.Count() ?? 0;
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    members = pageSize <= 0
                        ? members
                        : members
                            .Skip(skip)
                            .Take(pageSize);

                    users = members.AsEnumerable().Select(e =>
                    {
                        var friendType = _friendService.CalculateIsFriend(myUserId, e.User.Id);

                        return new SimpleTourMember(
                            e.User.Id,
                            e.User.Name,
                            e.User.Avatar,
                            e.User.Job,
                            friendType,
                            e.TourMember.JoinAt,
                            e.TourMember.AcceptedAt
                        );
                    }).ToList();
                }
                else
                {
                    users = new List<SimpleTourMember>();
                }

                pagination = new Pagination(total, page, pageSize);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool TryGetTourAcceptedMembers(int myUserId, int tourId, int page, int pageSize,
            out List<SimpleTourMember> users, out Pagination pagination)
        {
            try
            {
                DbService.ConnectDb(out _context);
                var members = (from tm in _context.TourMembers
                    join u in _context.Users on tm.UserId equals u.Id
                    where (tm.TourId == tourId && tm.DeletedAt == null && tm.AcceptedAt != null)
                    select new
                    {
                        User = u,
                        TourMember = tm,
                    });

                var total = members?.Count() ?? 0;
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    members = pageSize <= 0
                        ? members
                        : members
                            .Skip(skip)
                            .Take(pageSize);

                    users = members.AsEnumerable().Select(e =>
                    {
                        var friendType = _friendService.CalculateIsFriend(myUserId, e.User.Id);

                        return new SimpleTourMember(
                            e.User.Id,
                            e.User.Name,
                            e.User.Avatar,
                            e.User.Job,
                            friendType,
                            e.TourMember.JoinAt,
                            e.TourMember.AcceptedAt
                        );
                    }).ToList();
                }
                else
                {
                    users = new List<SimpleTourMember>();
                }

                pagination = new Pagination(total, page, pageSize);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool TryGetTour(int userId, int id, out Tour tour)
        {
            try
            {
                DbService.ConnectDb(out _context);
                tour = _context.Tours.SingleOrDefault(t => t.Id == id && t.DeletedAt == null) ??
                       throw new ExceptionWithMessage("Tour not found");
                var createById = tour.CreateById;
                var createBy = _context.Users.FirstOrDefault(u => u.Id == createById);
                var tourMember = _context.TourMembers.FirstOrDefault(tm => tm.TourId == id && tm.UserId == userId);
                var friendType = _friendService.CalculateIsFriend(userId, createById);

                TryGetTourInfo(userId, id, out var tourInfo);
                TryGetTimelines(id, out var timeLines);

                tour.TimeLines = timeLines;
                tour.TourInfo = tourInfo;
                tour.CreateBy = createBy?.ToSimpleUser(friendType);
                tour.JoinAt = tourMember?.JoinAt;
                tour.DeletedAt = tourMember?.DeletedAt;
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool TryGetTourInfo(int userId, int tourId, out TourInfo tourInfo)
        {
            try
            {
                DbService.ConnectDb(out _context);
                var tour = _context.Tours.SingleOrDefault(t => t.Id == tourId) ??
                           throw new ExceptionWithMessage("Tour not found");
                tourInfo = _context.TourInfos.FirstOrDefault(t => t.Id == tour.TourInfoId);

                var tourInfoDb = tourInfo;
                var startPlace = _context.Places.FirstOrDefault(p => p.Id == tourInfoDb.StartPlaceId);
                var destinationPlace = _context.Places.FirstOrDefault(p => p.Id == tourInfoDb.DestinatePlaceId);
                var createBy = _context.Users.FirstOrDefault(u => u.Id == tourInfoDb.CreateById);

                var friendType = _friendService.CalculateIsFriend(userId, createBy.Id);

                tourInfo.StartPlace = startPlace;
                tourInfo.DestinatePlace = destinationPlace;
                tourInfo.CreateBy = createBy.ToSimpleUser(friendType);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool TryGetTimelines(int tourId, out List<TimeLine> timelines)
        {
            try
            {
                DbService.ConnectDb(out _context);
                var _ = _context.Tours.SingleOrDefault(t => t.Id == tourId) ??
                        throw new ExceptionWithMessage("Tour not found");

                timelines = _context.TimeLines.Where(t => t.TourId == tourId)?.ToList() ?? new List<TimeLine>();

                foreach (var timeLine in timelines)
                {
                    var timelineDetails = _context.TimelineDetails.Where(td => td.TimelineId == timeLine.Id).ToList();
                    for (int i = 0; i < timelineDetails.Count; i++)
                    {
                        var place = _context.Places.FirstOrDefault(p => p.Id == timelineDetails[i].PlaceId);
                        timelineDetails[i].Place = place;
                    }

                    timeLine.TimelineDetails ??= new List<TimelineDetail>();
                    timeLine.TimelineDetails.AddRange(timelineDetails);
                }
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
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
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool TryJoinTour(Tour tour, User user)
        {
            try
            {
                DbService.ConnectDb(out _context);
                var tourMember = _context.TourMembers.FirstOrDefault(t =>
                    t.TourId == tour.Id && t.UserId == user.Id && t.DeletedAt == null);

                if (tour.CreateById == user.Id)
                {
                    throw new ExceptionWithMessage("Cannot perform request, you are creator");
                }

                if (tourMember != null)
                {
                    var exception = tourMember.AcceptedAt != null
                        ? new ExceptionWithMessage("You have already joined the tour")
                        : new ExceptionWithMessage("You have already requested to join the tour");
                    throw exception;
                }

                _context.TourMembers.Add(new TourMember(tour.Id, user.Id));
                _context.SaveChanges();
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool TryAcceptJoinTour(Tour tour, User user)
        {
            try
            {
                DbService.ConnectDb(out _context);
                var tourMember = _context.TourMembers.FirstOrDefault(t =>
                                     t.TourId == tour.Id && t.UserId == user.Id && t.AcceptedAt == null &&
                                     t.DeletedAt == null) ??
                                 throw new ExceptionWithMessage("Request not found");

                var totalMember = _context.TourMembers.Count(t => t.TourId == tour.Id && t.AcceptedAt != null);

                if (totalMember >= tour.MaxMember)
                {
                    throw new ExceptionWithMessage("The members are maximum");
                }

                tourMember.AcceptedAt = DateTime.Now;
                _context.TourMembers.Update(tourMember);
                _context.SaveChanges();
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
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
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
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
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
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
                DbService.DisconnectDb(ref _context);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool GetTopUser(int myUserId, int page, int pageSize, out List<SimpleUser> result,
            out Pagination pagination)
        {
            try
            {
                DbService.ConnectDb(out _context);

                result = new List<SimpleUser>();

                var tourGroup = from p in _context.Tours
                    where p.DeletedAt == null
                    group p by p.CreateById
                    into pg
                    select new
                    {
                        Key = pg.Key,
                        Count = pg.Count()
                    };

                var total = tourGroup.Count();
                var skip = pageSize * (page - 1);
                pageSize = pageSize <= 0 ? total : pageSize;

                result = tourGroup
                    .AsEnumerable()
                    .ToList()
                    .Skip(skip)
                    .Take(pageSize)
                    .Select((e) =>
                    {
                        var author = _context.Users.FirstOrDefault(u => u.Id == e.Key);
                        var friendType = _friendService.CalculateIsFriend(myUserId, author.Id);
                        var simpleAuthor = author.ToSimpleUser(friendType, e.Count);
                        return simpleAuthor;
                    }).ToList();

                result = result.OrderByDescending(e => e.TotalPost).ToList();

                pagination = new Pagination(total, page, pageSize);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool GetTourListRecommend(int userId, int page, int pageSize, out List<SimpleTour> tours,
            out Pagination pagination)
        {
            tours = null;
            pagination = null;
            bool isSuccess;

            try
            {
                DbService.ConnectDb(out _context);

                var toursDb = (from t in _context.Tours
                    join ti in _context.TourInfos on t.TourInfoId equals ti.Id
                    join tm in _context.TourMembers on
                        new
                        {
                            Id = t.Id,
                            UserId = userId
                        }
                        equals
                        new
                        {
                            Id = tm.TourId,
                            UserId = tm.UserId
                        } into tourInfoMember
                    from tim in tourInfoMember.DefaultIfEmpty()
                    join host in _context.Users on t.CreateById equals host.Id
                    where (
                        t.DeletedAt == null
                        && t.StartDay > DateTime.Now
                        && t.CreateById != userId
                        && tim == null)
                    select new
                    {
                        t.Id,
                        t.Name,
                        t.StartDay,
                        t.EndDay,
                        t.Price,
                        Host = host,
                        TourInfo = ti,
                        tim.JoinAt,
                        tim.AcceptedAt
                    }).AsEnumerable()?.Distinct((a, b) => a.Id == b.Id).ToList();

                // Sort by Create Date
                toursDb = toursDb.OrderByDescending(t => t.Id).ToList();

                var total = toursDb.Count();
                var skip = pageSize * (page - 1);
                pageSize = pageSize <= 0 ? total : pageSize;

                tours = toursDb
                    .Skip(skip)
                    .Take(pageSize)
                    .Select((e) =>
                    {
                        var totalMember = _context.TourMembers.Count(t => t.TourId == e.Id);

                        var friends = (from tourMember in _context.TourMembers
                                join friend in (from fr in _context.Friends.Where(fr =>
                                            fr.AcceptedAt != null &&
                                            (fr.UserId == userId || fr.RequestedUserId == userId))
                                        select new
                                        {
                                            Id = fr.UserId == userId ? fr.RequestedUserId : fr.UserId
                                        }
                                    ) on tourMember.UserId equals friend.Id
                                join user in _context.Users on friend.Id equals user.Id
                                where tourMember.TourId == e.Id
                                select user
                            )?.AsEnumerable().ToList();

                        var listFriend = friends.Any()
                            ? friends.Select(u => u.ToSimpleUser(FriendType.Accepted)).ToList()
                            : new List<SimpleUser>();

                        return new SimpleTour(
                            e.Id,
                            e.Name,
                            e.StartDay,
                            e.EndDay,
                            totalMember,
                            e.Host.ToSimpleUser(_friendService.CalculateIsFriend(userId, e.Host.Id)),
                            listFriend,
                            e.Price,
                            e.TourInfo,
                            e.JoinAt,
                            e.AcceptedAt);
                    })
                    .ToList();

                pagination = new Pagination(total, page, pageSize);
                isSuccess = true;
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return isSuccess;
        }

        public bool GetTourListForYou(int userId, int page, int pageSize, out List<SimpleTour> tours,
            out Pagination pagination)
        {
            tours = null;
            pagination = null;
            bool isSuccess;

            try
            {
                DbService.ConnectDb(out _context);

                var toursDb = (from t in _context.Tours
                    join ti in _context.TourInfos on t.TourInfoId equals ti.Id
                    join tm in _context.TourMembers on
                        new
                        {
                            Id = t.Id,
                            UserId = userId
                        }
                        equals
                        new
                        {
                            Id = tm.TourId,
                            UserId = tm.UserId
                        } into tourInfoMember
                    from tim in tourInfoMember.DefaultIfEmpty()
                    join host in _context.Users on t.CreateById equals host.Id
                    where (
                        t.DeletedAt == null
                        && t.StartDay > DateTime.Now
                        && t.CreateById != userId
                        && tim == null)
                    select new
                    {
                        t.Id,
                        t.Name,
                        t.StartDay,
                        t.EndDay,
                        t.Price,
                        Host = host,
                        TourInfo = ti,
                        tim.JoinAt,
                        tim.AcceptedAt
                    }).AsEnumerable()?.Distinct((a, b) => a.Id == b.Id).ToList();

                // Random list
                Random rnd = new Random();
                toursDb = toursDb.OrderBy(x => rnd.Next()).ToList();

                var total = toursDb.Count();
                var skip = pageSize * (page - 1);
                pageSize = pageSize <= 0 ? total : pageSize;

                tours = toursDb
                    .Skip(skip)
                    .Take(pageSize)
                    .Select((e) =>
                    {
                        var totalMember = _context.TourMembers.Count(t => t.TourId == e.Id);
                        var friends = (from tourMember in _context.TourMembers
                                join friend in (from fr in _context.Friends.Where(fr =>
                                            fr.AcceptedAt != null &&
                                            (fr.UserId == userId || fr.RequestedUserId == userId))
                                        select new
                                        {
                                            Id = fr.UserId == userId ? fr.RequestedUserId : fr.UserId
                                        }
                                    ) on tourMember.UserId equals friend.Id
                                join user in _context.Users on friend.Id equals user.Id
                                where tourMember.TourId == e.Id
                                select user
                            )?.AsEnumerable().ToList();

                        var listFriend = friends.Any()
                            ? friends.Select(u => u.ToSimpleUser(FriendType.Accepted)).ToList()
                            : new List<SimpleUser>();

                        return new SimpleTour(
                            e.Id,
                            e.Name,
                            e.StartDay,
                            e.EndDay,
                            totalMember,
                            e.Host.ToSimpleUser(_friendService.CalculateIsFriend(userId, e.Host.Id)),
                            listFriend,
                            e.Price,
                            e.TourInfo,
                            e.JoinAt,
                            e.AcceptedAt);
                    })
                    .ToList();

                pagination = new Pagination(total, page, pageSize);
                isSuccess = true;
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return isSuccess;
        }

        public bool GetTourListFriend(int userId, int page, int pageSize, out List<SimpleTour> tours,
            out Pagination pagination)
        {
            tours = null;
            pagination = null;
            bool isSuccess;

            try
            {
                DbService.ConnectDb(out _context);

                var toursDb = (from t in _context.Tours
                    join ti in _context.TourInfos on t.TourInfoId equals ti.Id
                    join tm in _context.TourMembers on
                        new
                        {
                            Id = t.Id,
                            UserId = userId
                        }
                        equals
                        new
                        {
                            Id = tm.TourId,
                            UserId = tm.UserId
                        } into tourInfoMember
                    from tim in tourInfoMember.DefaultIfEmpty()
                    join host in _context.Users on t.CreateById equals host.Id
                    where (
                        t.DeletedAt == null
                        && t.StartDay > DateTime.Now
                        && t.CreateById != userId
                        && tim == null)
                    select new
                    {
                        t.Id,
                        t.Name,
                        t.StartDay,
                        t.EndDay,
                        t.Price,
                        Host = host,
                        TourInfo = ti,
                        tim.JoinAt,
                        tim.AcceptedAt
                    }).AsEnumerable()?.Distinct((a, b) => a.Id == b.Id).ToList();

                // Get list friends
                tours = toursDb.Select((e) =>
                    {
                        var totalMember = _context.TourMembers.Count(t => t.TourId == e.Id);
                        var friends = (from tourMember in _context.TourMembers
                                join friend in (from fr in _context.Friends.Where(fr =>
                                            fr.AcceptedAt != null &&
                                            (fr.UserId == userId || fr.RequestedUserId == userId))
                                        select new
                                        {
                                            Id = fr.UserId == userId ? fr.RequestedUserId : fr.UserId
                                        }
                                    ) on tourMember.UserId equals friend.Id
                                join user in _context.Users on friend.Id equals user.Id
                                where tourMember.TourId == e.Id
                                select user
                            )?.AsEnumerable().ToList();

                        var listFriend = friends.Any()
                            ? friends.Select(u => u.ToSimpleUser(FriendType.Accepted)).ToList()
                            : new List<SimpleUser>();

                        return new SimpleTour(
                            e.Id,
                            e.Name,
                            e.StartDay,
                            e.EndDay,
                            totalMember,
                            e.Host.ToSimpleUser(_friendService.CalculateIsFriend(userId, e.Host.Id)),
                            listFriend,
                            e.Price,
                            e.TourInfo,
                            e.JoinAt,
                            e.AcceptedAt);
                    })
                    .ToList();

                // Random list
                var rnd = new Random();
                tours = tours
                    .Where(e => e.Friends.Count > 0)
                    .OrderBy(x => rnd.Next())
                    .ToList();

                var total = tours.Count();
                var skip = pageSize * (page - 1);
                pageSize = pageSize <= 0 ? total : pageSize;

                tours = tours
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                pagination = new Pagination(total, page, pageSize);
                isSuccess = true;
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return isSuccess;
        }
    }
}