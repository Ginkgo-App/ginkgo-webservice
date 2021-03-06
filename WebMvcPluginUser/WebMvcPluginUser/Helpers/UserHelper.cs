﻿using System;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace WebMvcPluginUser.Helpers
{
    static class UserHelper
    {
        public static string HashPassword(string password)
        {
            var salt = Encoding.ASCII.GetBytes(APICore.Vars.PasswordSalt);

            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hashed;
        }
    }
}