using System;
using System.Collections.Generic;
using System.Text;

namespace WebMvcPluginUser.Models
{
    class UserToken
    {
        public UserToken(string token)
        {
            Token = token;
        }

        public string Token { get; set; }
    }
}
