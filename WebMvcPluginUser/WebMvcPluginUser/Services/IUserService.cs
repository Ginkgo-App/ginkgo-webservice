using APICore.Helpers;
using System.Collections.Generic;
using WebMvcPluginUser.Entities;

namespace WebMvcPluginUser.Services
{
    public interface IUserService
    {
        ErrorList.ErrorCode Authenticate(string username, string password, out User user);
        bool TryGetUsers(out List<User> users);
        bool TryGetUsers(string userId, out User user);
    }
}