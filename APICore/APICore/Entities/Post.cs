#nullable enable
using System;
using APICore.Models;

namespace APICore.Entities
{
    public class Post : IIsDeleted
    {
        public int Id { get; private set; }
        public string? Content { get; private set; }
        public string[]? Images { get; private set; }
        public int AuthorId { get; private set; }
        public DateTime CreateAt { get; private set; }
        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}