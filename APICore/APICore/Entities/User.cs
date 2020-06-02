#nullable enable
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using APICore.Helpers;
using APICore.Models;

namespace APICore.Entities
{
    public class User : IIsDeleted
    {
        public User(string email, string role)
        {
            Email = email;
            Role = role;
        }

        public User(string password, string email, string role, string name, string phoneNumber = null,
            string fullName = null, string avatar = null, string bio = null, string slogan = null, string job = null,
            DateTime? birthday = null, string gender = null, string address = null)
        {
            Name = name;
            Password = CoreHelper.HashPassword(password);
            Email = CoreHelper.ValidateEmail(email);
            PhoneNumber = phoneNumber;
            FullName = fullName;
            Avatar = avatar;
            Bio = bio;
            Slogan = slogan;
            Job = job;
            Birthday = birthday ?? new DateTime();
            Gender = gender;
            Address = address;
            Role = RoleType.TryParse(role) ?? RoleType.User;
        }

        public void Update(string? name, string? password, string email, string? phoneNumber, string? avatar,
            string? bio, string? slogan, string? job, DateTime? birthday, string? gender, string? address, string? role)
        {
            Name = name ?? Name;
            Password = password != null ? CoreHelper.HashPassword(password) : Password;
            Email = email != null ? CoreHelper.ValidateEmail(email) : Email;
            PhoneNumber = phoneNumber ?? PhoneNumber;
            Avatar = avatar ?? Avatar;
            Bio = bio ?? Bio;
            Slogan = slogan ?? Slogan;
            Job = job ?? Job;
            Birthday = birthday ?? Birthday;
            Gender = gender ?? Gender;
            Address = address ?? Address;
            // If role diff Null, try parse to correct role, else role = default(user)
            Role = (role != null ? RoleType.TryParse(role) : Role) ?? RoleType.User;
        }

        public int Id { get; private set; }
        public string? Name { get; private set; }
        public string? Password { get; private set; }
        [NotMapped] public string? Token { get; set; }
        public string Email { get; private set; }
        public string? PhoneNumber { get; private set; }
        public string? FullName { get; private set; }
        public string? Avatar { get; private set; }
        public string? Bio { get; private set; }
        public string? Slogan { get; private set; }
        public string? Job { get; private set; }
        public DateTime? Birthday { get; private set; }
        public string? Gender { get; private set; }
        public string? Address { get; private set; }
        public string Role { get; private set; }


        public JObject ToSimpleJson(string friendType)
        {
            var simpleUser = new SimpleUser(Id, Name, Avatar, Job, FriendType.TryParse(friendType));
            return JObject.FromObject(simpleUser);
        }

        public SimpleUser ToSimpleUser(string friendType)
        {
            var simpleUser = new SimpleUser(Id, Name, Avatar, Job, FriendType.TryParse(friendType));
            return simpleUser;
        }

        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
    
    public static class RoleType
    {
        public const string Creator = "creator";
        public const string User = "user";
        public const string Admin = "admin";

        public static string? TryParse(string text)
        {
            return text?.ToLower() switch
            {
                Creator => Creator,
                Admin => Admin,
                User => User,
                _ => User
            };
        }
    }

    public static class GenderType
    {
        public const string Male = "male";
        public const string Female = "female";
        public const string Other = "other";

        public static string? TryParse(string text)
        {
            return text?.ToLower() switch
            {
                "male" => Male,
                "female" => Female,
                "other" => Other,
                _ => Other
            };
        }
    }

    public static class FriendType
    {
        public const string Me = "me";
        public const string None = "none";
        public const string Accepted = "accepted";
        public const string Waiting = "waiting";
        public const string Requested = "requested";

        public static string TryParse(string type)
        {
            return type?.ToLower() switch
            {
                "me" => Me,
                "none" => None,
                "accepted" => Accepted,
                "waiting" => Waiting,
                "requested" => Requested,
                _ => None
            };
        }
    }
}
