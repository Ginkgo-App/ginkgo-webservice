using APICore.Entities;
using APICore.Helpers;
using System.Collections.Generic;

namespace APICore.Services
{
    public interface IUserService
    {
        ErrorList.ErrorCode Authenticate(string email, ref AuthProvider authProvider, out User user);
        ErrorList.ErrorCode Authenticate(string email, string password, out User user);
        ErrorList.ErrorCode Register(string name, string email, string phoneNumber, string password, out User user);
        bool TryAddAuthProvider(AuthProvider authProvider, User user);
        bool TryAddUser(User user);
        bool TryGetAuthProvider(string id, out AuthProvider authProvider);
        bool TryGetFacbookInfo(string accessToken, out AuthProvider authProvider);
        bool TryGetFriends(int userId, out List<User> friends);
        bool TryGetTourInfoById(int tourId, out TourInfo tourInfos);
        bool TryGetTours(int userId, out List<TourInfo> tourInfos);
        bool TryGetUsers(int page, int pageSize, out List<User> users, out Pagination pagination);
        bool TryGetUsers(int userId, out User user);
        bool TryGetUsers(string email, out User user);
        bool TryRemoveTourInfo(int tourInfoId);
        bool TryRemoveUser(int userId);
        bool TryUpdateTourInfo(TourInfo tourInfo);
        bool TryUpdateUser(User user);
    }
}