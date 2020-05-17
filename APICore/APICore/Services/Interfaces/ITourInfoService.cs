using APICore.Entities;
using APICore.Helpers;
using System.Collections.Generic;
using APICore.Models;

namespace APICore.Services
{
    public interface ITourInfoService
    {
        bool TryAddTourInfo(TourInfo tourInfo);
        ErrorList.ErrorCode TryGetPlaceById(int placeId, out Place place);
        ErrorList.ErrorCode TryGetServiceById(int serviceId, out Service service);
        ErrorList.ErrorCode TryGetTourInfoById(int tourId, out TourInfo tourInfos);
        ErrorList.ErrorCode TryGetTourInfos(int page, int pageSize, out List<TourInfo> tourInfos, out Pagination pagination);
        ErrorList.ErrorCode TryGetTours(int tourInfoId, int page, int pageSize, out List<Tour> tourInfos, out Pagination pagination);
        ErrorList.ErrorCode TryGetToursByUserId(int userId, int page, int pageSize, out List<TourInfo> tourInfos, out Pagination pagination);
        ErrorList.ErrorCode TryGetUserById(int userId, out User user);
        bool TryRemoveTourInfo(int tourInfoId);
        bool TryUpdateTourInfo(TourInfo tourInfo);
    }
}