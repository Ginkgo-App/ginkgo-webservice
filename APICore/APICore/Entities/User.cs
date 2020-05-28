#nullable enable
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using APICore.Helpers;
using APICore.Models;

namespace APICore.Entities
{
    public class User
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
            JObject result = new JObject
            {
                ["Id"] = this.Id,
                ["Name"] = this.Name,
                ["Avatar"] = this.Avatar,
                ["Job"] = this.Job,
                ["FriendType"] = friendType
            };

            return result;
        }

        public SimpleUser ToSimpleUser(string friendType)
        {
            var simpleUser = new SimpleUser(Id, Name, Avatar, Job, friendType);

            return simpleUser;
        }
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
        if (text == null)
        {
            return null;
        }

        if (text.Equals(Male, StringComparison.OrdinalIgnoreCase))
        {
            return Male;
        }
        else if (text.Equals(Female, StringComparison.OrdinalIgnoreCase))
        {
            return Female;
        }
        else if (text.Equals(Other, StringComparison.OrdinalIgnoreCase))
        {
            return Other;
        }

        return null;
    }
}

public static class FriendType
{
    public const string None = "none";
    public const string Accepted = "accepted";
    public const string Requested = "requested";
    public const string Waiting = "waiting";

    public static string? TryParse(string text)
    {
        if (text == null)
        {
            return null;
        }

        if (text.Equals(None, StringComparison.OrdinalIgnoreCase))
        {
            return None;
        }
        else if (text.Equals(Accepted, StringComparison.OrdinalIgnoreCase))
        {
            return Accepted;
        }
        else if (text.Equals(Requested, StringComparison.OrdinalIgnoreCase))
        {
            return Requested;
        }
        else if (text.Equals(Waiting, StringComparison.OrdinalIgnoreCase))
        {
            return Waiting;
        }

        return null;
    }
}