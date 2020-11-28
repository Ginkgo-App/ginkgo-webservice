#nullable enable
using System;
using APICore.Models;

namespace APICore.Entities
{
    public class Message : IIsDeleted
    {
        public int Id { get; private set; }
        public int GroupId { get; private set; }
        public string? Content { get; private set; }
        public string[]? Images { get; private set; }
        public int CreateBy { get; private set; }
        public DateTime CreateAt { get; private set; }
        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
