using System.Collections.Generic;
using APICore.Entities;
using APICore.Models;

namespace APICore.Services.Interfaces
{
    public interface IPlaceService
    {
        bool TryGetAllPlaces(int page, int pageSize, out List<Place> places, out Pagination pagination);
        bool TryGetPlaceById(int placeId, out Place place);
    }
}