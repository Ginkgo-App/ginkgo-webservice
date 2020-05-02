using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebMvcPluginUser.Entities;

namespace APICore.Entities
{
    public class AuthProvider
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
        public string Provider { get; set; }
        public User User { get; set; }
    }

    public enum ProviderType
    {
        facebook,
        google
    }
}