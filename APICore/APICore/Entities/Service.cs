using System;
using APICore.Models;
using Newtonsoft.Json.Linq;

#nullable enable
namespace APICore.Entities
{
    public class Service : IIsDeleted
    {
        public Service(string? name, string? image, DateTime? deletedAt)
        {
            Name = name;
            Image = image;
            DeletedAt = deletedAt;
        }

        public int Id { get; private set; }
        public string? Name { get; private set; }
        public string? Image { get; private set; }
        public DateTime? DeletedAt { get; set; }

        public JObject ToJson()
        {
            return JObject.FromObject(this);
        }

        public void Update(string name, string image)
        {
            Name = name ?? Name;
            Image = image ?? image;
            return;
        }
        
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
