using APICore.Entities;
using System;
using System.Collections.Generic;

namespace WebMvcPluginUser.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public ICollection<AuthProvider> AuthProviders { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public string Bio { get; set; }
        public string Job { get; set; }
        public DateTime Birthday { get; set; }
        public Gender Gender { get; set; }
        public string Address { get; set; }
        public User[] Friends { get; set; }
        public Post[] Diaries { get; set; }
        public Tour[] Tours { get; set; }
    }
}
