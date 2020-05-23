#nullable enable
using APICore.Entities;

namespace APICore.Entities
{
    public class AuthProvider
    {
        public AuthProvider(string id, string? name, string? email, string? avatar, string? provider, int userId)
        {
            Id = id;
            Name = name;
            Email = email;
            Avatar = avatar;
            Provider = provider;
            UserId = userId;
        }

        public AuthProvider()
        {
        }

        public string Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public string? Provider { get; set; }
        public int UserId { get; set; }
    }

    public enum ProviderType
    {
        facebook,
        google
    }
}