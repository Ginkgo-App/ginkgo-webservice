using APICore.Entities;
using APICore.Helpers;
using System.Collections.Generic;

namespace WebMvcPluginTour.Services
{
    public interface ITourInfoService
    {
        bool TryAddTourInfo(TourInfo tourInfo);
        ErrorList.ErrorCode TryGetPlaceById(int placeId, out Place place);
        ErrorList.ErrorCode TryGetTourInfoById(int tourId, out TourInfo tourInfos);
        ErrorList.ErrorCode TryGetTours(int page, int pageSize, out List<TourInfo> tourInfos, out Pagination pagination);
        ErrorList.ErrorCode TryGetToursByUserId(int userId, int page, int pageSize, out List<TourInfo> tourInfos, out Pagination pagination);
        ErrorList.ErrorCode TryGetUserById(int userId, out User user);
        bool TryRemoveTourInfo(int tourInfoId);
        bool TryUpdateTourInfo(TourInfo tourInfo);
    }
}