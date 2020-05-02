using System.Collections.Generic;
using System.Linq;
using WebMvcPluginUser.Entities;

namespace WebMvcPluginUser.Helpers
{
    class UserHelper
    {
        public IEnumerable<User> WithoutPasswords(IEnumerable<User> users)
        {
            return users.Select(x => this.WithoutPassword(x));
        }

        public User WithoutPassword(User user)
        {
            var result = new User
            {
                Id = user.Id,
                Name = user.Name,
                Password = null,
                Address = user.Address,
                Avatar = user.Avatar,
                Bio = user.Bio,
                Birthday = user.Birthday,
                Diaries = user.Diaries,
                Email = user.Email,
                Friends = user.Friends,
                FullName = user.FullName,
                Gender = user.Gender,
                Job = user.Job,
                PhoneNumber = user.PhoneNumber,
                AuthProviders = user.AuthProviders,
                Token = user.Token,
                Tours = user.Tours,
            };
            return result;
        }
    }
}
