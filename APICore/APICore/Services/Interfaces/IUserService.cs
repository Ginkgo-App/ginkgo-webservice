using System.Collections.Generic;
using APICore.Entities;
using APICore.Helpers;
using APICore.Models;

namespace APICore.Services
{
    public interface IUserService
    {
        ErrorList.ErrorCode Authenticate(string email, string password, out User user);
        ErrorList.ErrorCode Authenticate(string email, ref AuthProvider authProvider, out User user);
        ErrorList.ErrorCode Register(string name, string email, string phoneNumber, string password, out User user);
        bool TryGetUsers(int page, int pageSize, out List<User> users, out Pagination pagination);
        bool TryGetUsers(string email, out User user);
        bool TryGetUsers(int userId, out User user);
        bool TryAddUser(User user);
        bool TryUpdateUser(User user);
        bool TryRemoveUser(int userId);
        bool TryAddAuthProvider(AuthProvider authProvider, User user);
        bool TryGetFacebookInfo(string accessToken, out AuthProvider authProvider);
        bool TryGetTours(int userId, out List<TourInfo> tourInfos);
        bool TryGetTourInfoById(int tourId, out TourInfo tourInfos);
        bool TryUpdateTourInfo(TourInfo tourInfo);
        bool TryRemoveTourInfo(int tourInfoId);
        bool TryGetFriends(int userId, out List<User> friends);
    }
}