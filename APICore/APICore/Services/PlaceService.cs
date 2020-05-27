using System.Collections.Generic;
using System.Linq;
using APICore.DBContext;
using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using Microsoft.Extensions.Options;
using NLog;

namespace APICore.Services
{
    public interface IPlaceService
    {
        bool TryGetAllPlaces(int page, int pageSize, out List<Place> places, out Pagination pagination);
        bool TryGetPlaceById(int placeId, out Place place);
        bool TryUpdatePlace(Place place);
        bool TryAddPlace(Place place);
        bool TryRemovePlace(int placeId);
    }

    public class PlaceService : IPlaceService
    {
        private PostgreSQLContext _context;
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.Logger;

        public PlaceService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public bool TryGetAllPlaces(int page, int pageSize, out List<Place> places, out Pagination pagination)
        {
            places = null;
            pagination = null;
            var isSuccess = false;

            try
            {
                CoreHelper.ValidatePageSize(ref page, ref pageSize);

                DbService.ConnectDb(out _context);
                var listTourInfos = _context.Places.ToList();

                var total = listTourInfos.Select(p => p.Id).Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    places = listTourInfos.Select(u => u)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToList();
                }
                else
                {
                    places = new List<Place>();
                }

                pagination = new Pagination(total, page, pageSize);
                isSuccess = true;
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return isSuccess;
        }

        public bool TryGetPlaceById(int placeId, out Place place)
        {
            place = null;

            try
            {
                DbService.ConnectDb(out _context);
                place = _context.Places.FirstOrDefault(p => p.Id == placeId);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryUpdatePlace(Place place)
        {
            try
            {
                DbService.ConnectDb(out _context);
                _context.Places.Update(place);
                _context.SaveChanges();
                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryAddPlace(Place place)
        {
            try
            {
                DbService.ConnectDb(out _context);
                _context.Places.Add(place);
                _context.SaveChanges();
                DbService.DisconnectDb(out _context);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryRemovePlace(int placeId)
        {
            var isSuccess = false;
            try
            {
                DbService.ConnectDb(out _context);
                var place = _context.Places.FirstOrDefault(p => p.Id == placeId);
                if (place != null)
                {
                    _context.Places.Remove(place);
                    _context.SaveChanges();
                    isSuccess = true;
                }
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return isSuccess;
        }
    }
}