using APICore.Entities;
using APICore.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebMvcPluginUser.Entities;

namespace WebMvcPluginUser.Services
{
    public interface IUserService
    {
        ErrorList.ErrorCode Authenticate(string email, string password, out User user);
        ErrorList.ErrorCode Authenticate(string email, ref AuthProvider authProvider, out User user);
        ErrorList.ErrorCode Register(string name, string email, string phoneNumber, string password, out User user);
        bool TryAddUser(User user);
        bool TryAddAuthProvider(AuthProvider authProvider, User user);
        bool TryGetFacbookInfo(string accessToken, out AuthProvider authProvider);
        bool TryGetUsers(int userId, out User user);
        bool TryGetUsers(out List<User> users);
        bool TryGetUsers(string email, out User user);
    }
}