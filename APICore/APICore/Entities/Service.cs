using System;
using APICore.Models;

#nullable enable
namespace APICore.Entities
{
    public class Service : IIsDeleted
    {
        public int Id { get; private set; }
        public string? Name { get; private set; }
        public string? Image { get; private set; }
        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
