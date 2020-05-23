using System.Collections.Generic;
using System.Linq;
using APICore.DBContext;
using APICore.Entities;
using APICore.Helpers;
using APICore.Models;

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

        public bool TryGetAllPlaces(int page, int pageSize, out List<Place> places, out Pagination pagination)
        {
            places = null;
            pagination = null;
            var isSuccess = false;

            try
            {
                CoreHelper.ValidatePageSize(ref page, ref pageSize);

                ConnectDb();
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
                DisconnectDb();
            }

            return isSuccess;
        }

        public bool TryGetPlaceById(int placeId, out Place place)
        {
            place = null;

            try
            {
                ConnectDb();
                place = _context.Places.FirstOrDefault(p => p.Id == placeId);
            }
            finally
            {
                DisconnectDb();
            }

            return true;
        }
        
        public bool TryUpdatePlace(Place place)
        {
            try
            {
                ConnectDb();
                _context.Places.Update(place);
                _context.SaveChanges();
                DisconnectDb();
            }
            finally
            {
                DisconnectDb();
            }

            return true;
        }
        
        public bool TryAddPlace(Place place)
        {
            try
            {
                ConnectDb();
                _context.Places.Add(place);
                _context.SaveChanges();
                DisconnectDb();
            }
            finally
            {
                DisconnectDb();
            }

            return true;
        }
        
        public bool TryRemovePlace(int placeId)
        {
            var isSuccess = false;
            try
            {
                ConnectDb();
                var place = _context.Places.FirstOrDefault(p => p.Id == placeId);
                if (place != null)
                {
                    _context.Places.Remove(place);
                    isSuccess = true;
                }
                
            }
            finally
            {
                DisconnectDb();
            }

            return isSuccess;
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