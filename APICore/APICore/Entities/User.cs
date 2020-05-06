using System;

namespace WebMvcPluginUser.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public string Bio { get; set; }
        public string Slogan { get; set; }
        public string Job { get; set; }
        public DateTime Birthday { get; set; }
        public Gender Gender { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
    }

    public static class RoleType
    {
        public const string Creator = "creator";
        public const string User = "user";
        public const string Admin = "admin";
    }
}
