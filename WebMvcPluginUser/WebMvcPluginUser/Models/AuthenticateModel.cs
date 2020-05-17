using System.ComponentModel.DataAnnotations;

namespace WebMvcPluginUser.Models
{
    public class AuthenticateModel
    {
        public AuthenticateModel(string email, string password)
        {
            Email = email;
            Password = password;
        }

        [Required] public string Email { get; }

        [Required] public string Password { get; }
    }
}