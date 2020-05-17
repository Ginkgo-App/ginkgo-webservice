using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace APICore.Entities
{
    public class User
    {
        public User()
        {
        }

        public User(string name, string hashPassword, string email, string? phoneNumber, string? fullName, string? avatar, string? bio, string? slogan, string? job, DateTime? birthday, string? gender, string? address, string role)
        {
            Name = name;
            Password = hashPassword;
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
            Role = role;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        [NotMapped]
        public string Token { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public string Bio { get; set; }
        public string Slogan { get; set; }
        public string Job { get; set; }
        public DateTime Birthday { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
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

        public static string  TryParse(string text)
        {
            if (text == null)
            {
                return null;
            }
            if (text.Equals(Creator, StringComparison.OrdinalIgnoreCase))
            {
                return Creator;
            }
            else if(text.Equals(Admin, StringComparison.OrdinalIgnoreCase))
            {
                return Admin;
            }
            else if(text.Equals(User, StringComparison.OrdinalIgnoreCase))
            {
                return User;
            }
            return null;
        }
    }

    public static class GenderType
    {
        public const string Male = "male";
        public const string Female = "female";
        public const string Other = "other";

        public static string  TryParse(string text)
        {
            if (text == null)
            {
                return null;
            }
            if (text.Equals(Male, StringComparison.OrdinalIgnoreCase))
            {
                return Male;
            }
            else if(text.Equals(Female, StringComparison.OrdinalIgnoreCase))
            {
                return Female;
            }
            else if(text.Equals(Other, StringComparison.OrdinalIgnoreCase))
            {
                return Other;
            }
            return null;
        }
    }
