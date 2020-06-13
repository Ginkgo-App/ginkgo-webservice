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
        bool TryGetAllPlaces(int page, int pageSize, out List<PlaceInfo> places, out Pagination pagination);
        bool TryGetPlaceInfoById(int placeId, out PlaceInfo place);
        bool TryGetPlaceById(int placeId, out Place place);
        bool TryUpdatePlace(Place place);
        bool TryAddPlace(Place place, int? parentId);
        bool TryRemovePlace(int placeId);
        bool TryGetPlaceTypeById(int placeId, out PlaceType placeType);
        bool TryUpdatePlaceTypeById(PlaceType placeType);
        bool TryRemovePlaceType(PlaceType placeType);
        bool TryRemovePlaceType(int placeTypeId);
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

        #region Place

        public bool TryGetAllPlaces(int page, int pageSize, out List<PlaceInfo> places, out Pagination pagination)
        {
            places = new List<PlaceInfo>();
            pagination = null;
            bool isSuccess;

            try
            {
                CoreHelper.ValidatePageSize(ref page, ref pageSize);

                DbService.ConnectDb(out _context);
                var listTourInfos = _context.Places.Where(t => t.DeletedAt == null).ToList();

                var total = listTourInfos.Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    // If pageSize = 0 => Get all
                    var placeIds = pageSize <= 0
                        ? listTourInfos
                        : listTourInfos
                            .Skip(skip)
                            .Take(pageSize)
                            .ToList();

                    foreach (var place in placeIds)
                    {
                        var placeType = _context.PlaceTypes.FirstOrDefault(t => t.Id == place.TypeId);
                        if (placeType == null) continue;
                        var childPlaces = new List<PlaceInfo>();

                        if (placeType.IsHaveChild)
                        {
                            var childPlaceIds = _context.ChildPlaces.Where(p => p.ParentId == place.Id).ToList();

                            foreach (var childPlaceId in childPlaceIds)
                            {
                                var childPlace = _context.Places.FirstOrDefault(p => p.Id == childPlaceId.ChildId);
                                var type = _context.PlaceTypes.FirstOrDefault(t => t.Id == childPlace.TypeId);
                                childPlaces.Add(new PlaceInfo(childPlace, type, new List<PlaceInfo>()));
                            }
                        }

                        places.Add(new PlaceInfo(place, placeType, childPlaces));
                    }
                }
                else
                {
                    places = new List<PlaceInfo>();
                }

                pagination = new Pagination(total, page, pageSize > 0 ? pageSize : total);
                isSuccess = true;
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return isSuccess;
        }

        public bool TryGetPlaceInfoById(int placeId, out PlaceInfo place)
        {
            place = null;
            var childPlaces = new List<PlaceInfo>();

            try
            {
                DbService.ConnectDb(out _context);
                var placeDb = _context.Places.FirstOrDefault(p => p.Id == placeId);
                if (placeDb == null) return false;

                var placeType = _context.PlaceTypes.FirstOrDefault(t => t.Id == placeDb.TypeId);
                if (placeType == null) return false;

                if (placeType.IsHaveChild)
                {
                    var childPlaceIds = _context.ChildPlaces.Where(p => p.ParentId == placeDb.Id).ToList();

                    foreach (var childPlaceId in childPlaceIds)
                    {
                        var childPlace = _context.Places.FirstOrDefault(p => p.Id == childPlaceId.ChildId);
                        var type = _context.PlaceTypes.FirstOrDefault(t => t.Id == childPlace.TypeId);
                        childPlaces.Add(new PlaceInfo(childPlace, type, new List<PlaceInfo>()));
                    }
                }

                place = new PlaceInfo(placeDb, placeType, childPlaces);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
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
                DbService.DisconnectDb(ref _context);
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
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool TryAddPlace(Place place, int? parentId)
        {
            try
            {
                DbService.ConnectDb(out _context);
                if (parentId != null)
                {
                    _ = _context.Places.FirstOrDefault(p => p.Id == parentId) ??
                        throw new ExceptionWithMessage("Parent place not found");
                }

                place = _context.Places.Add(place).Entity;
                _context.SaveChanges();


                var childPlaceRecord =
                    _context.ChildPlaces.FirstOrDefault(p => p.ChildId == place.Id && p.ParentId == parentId);

                if (childPlaceRecord == null && parentId != null)
                {
                    _context.ChildPlaces.Add(new ChildPlace((int) parentId, place.Id));
                }
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
                DbService.ConnectDb(out _context);
                var place = _context.Places.FirstOrDefault(p => p.Id == placeId);
                if (place != null)
                {
                    place.Delete();
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

        #endregion

        #region PlaceType

        public bool TryGetPlaceTypeById(int placeId, out PlaceType placeType)
        {
            placeType = null;

            try
            {
                DbService.ConnectDb(out _context);
                placeType = _context.PlaceTypes.FirstOrDefault(p => p.Id == placeId);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool TryUpdatePlaceTypeById(PlaceType placeType)
        {
            try
            {
                DbService.ConnectDb(out _context);
                _ = _context.PlaceTypes.Update(placeType);
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool TryRemovePlaceType(PlaceType placeType)
        {
            try
            {
                DbService.ConnectDb(out _context);
                placeType.Delete();
                _context.SaveChanges();
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        public bool TryRemovePlaceType(int placeTypeId)
        {
            try
            {
                DbService.ConnectDb(out _context);
                var placeType = _context.PlaceTypes.FirstOrDefault(p => p.Id == placeTypeId);

                if (placeType == null)
                {
                    throw new ExceptionWithMessage("Place type not found");
                }

                placeType.Delete();
                _context.SaveChanges();
            }
            finally
            {
                DbService.DisconnectDb(ref _context);
            }

            return true;
        }

        #endregion
    }
}