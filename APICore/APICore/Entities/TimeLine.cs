#nullable enable
using System;
using APICore.Models;

namespace APICore.Entities
{
    public class TimeLine : IIsDeleted
    {
        public int Id { get; private set; }
        public DateTime Day { get; private set; }
        public string? Description { get; private set; }
        public int TourId { get; private set; }
        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
