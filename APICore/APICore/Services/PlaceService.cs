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

        public PlaceService(IOptions<AppSettings> appSettings, PostgreSQLContext context)
        {
            _appSettings = appSettings.Value;
            _context = context;
        }

        public bool TryGetAllPlaces(int page, int pageSize, out List<Place> places, out Pagination pagination)
        {
            places = null;
            pagination = null;
            var isSuccess = false;

            try
            {
                CoreHelper.ValidatePageSize(ref page, ref pageSize);

                DbService.ConnectDb(ref _context);
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

                    pagination = new Pagination(total, page, pageSize);
                    isSuccess = true;
                }
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return isSuccess;
        }

        public bool TryGetPlaceById(int placeId, out Place place)
        {
            place = null;

            try
            {
                DbService.ConnectDb(ref _context);
                place = _context.Places.FirstOrDefault(p => p.Id == placeId);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }
        
        public bool TryUpdatePlace(Place place)
        {
            try
            {
                DbService.ConnectDb(ref _context);
                _context.Places.Update(place);
                _context.SaveChanges();
                DbService.DisconnectDb(ref _context);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }
        
        public bool TryAddPlace(Place place)
        {
            try
            {
                DbService.ConnectDb(ref _context);
                _context.Places.Add(place);
                _context.SaveChanges();
                DbService.DisconnectDb(ref _context);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }
        
        public bool TryRemovePlace(int placeId)
        {
            var isSuccess = false;
            try
            {
                DbService.ConnectDb(ref _context);
                var place = _context.Places.FirstOrDefault(p => p.Id == placeId);
                if (place != null)
                {
                    _context.Places.Remove(place);
                    isSuccess = true;
                }
                
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return isSuccess;
        }
    }
}