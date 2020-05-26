#nullable enable
using APICore.Entities;

namespace APICore.Entities
{
    public class AuthProvider
    {
        public AuthProvider(string id, string? name = null, string? email = null, string? avatar = null,
            string? provider = null, int userId = 0)
        {
            Id = id;
            Name = name;
            Email = email;
            Avatar = avatar;
            Provider = provider;
            UserId = userId;
        }

        public string Id { get; private set; }
        public string? Name { get; private set; }
        public string? Email { get; private set; }
        public string? Avatar { get; private set; }
        public string? Provider { get; private set; }
        public int UserId { get; set; }
    }

    public enum ProviderType
    {
        facebook,
        google
    }
}