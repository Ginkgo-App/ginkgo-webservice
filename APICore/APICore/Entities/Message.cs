#nullable enable
using System;
using APICore.Models;

namespace APICore.Entities
{
    public class Message : IIsDeleted
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string? Content { get; set; }
        public string[]? Images { get; set; }
        public int CreateBy { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
