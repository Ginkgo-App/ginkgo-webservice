using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using APICore.Entities;

namespace APICore.Helpers
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
                Email = user.Email,
                FullName = user.FullName,
                Gender = user.Gender,
                Job = user.Job,
                PhoneNumber = user.PhoneNumber,
                Token = user.Token,
                Role = user.Role,
                Slogan = user.Slogan,
            };
            return result;
        }

        public static string HashPassword(string password)
        {
            byte[] salt = Encoding.ASCII.GetBytes(APICore.Vars.PASSWORD_SALT);

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                            password: password,
                            salt: salt,
                            prf: KeyDerivationPrf.HMACSHA1,
                            iterationCount: 10000,
                            numBytesRequested: 256 / 8));

            return hashed;
        }
    }
}
