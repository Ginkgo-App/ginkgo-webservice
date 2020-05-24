#nullable enable
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;

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
            Password = Helpers.CoreHelper.HashPassword(password);
            Email = email;
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

        public User Update(string? name, string? password, string email, string? phoneNumber, string? avatar, string? bio, string? slogan, string? job, DateTime? birthday, string? gender, string? address, string? role)
        {
            Name = name ?? Name;
            Password = password != null ? Helpers.CoreHelper.HashPassword(password) : Password;
            Email = email ?? Email;
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
            return this;
        }

        // public User()
        // {
        // }

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        [NotMapped] 
        public string? Token { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public string? Bio { get; set; }
        public string? Slogan { get; set; }
        public string? Job { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string Role { get; set; }


        public JObject ToSimpleJson(int isFriend)
        {
            JObject result = new JObject
            {
                ["Id"] = this.Id,
                ["Name"] = this.Name,
                ["Avatar"] = this.Avatar,
                ["Job"] = this.Job,
                ["IsFriend"] = isFriend
            };

            return result;
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