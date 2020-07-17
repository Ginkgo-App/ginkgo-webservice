#nullable enable
using System;
using APICore.Models;

namespace APICore.Entities
{
    public class FeedbackComment : IIsDeleted
    {
        public int Id { get; private set; }
        public string? Content { get; private set; }
        public int UserId { get; private set; }
        public DateTime CreateAt { get; private set; }
        public int TourInfoId { get; private set; }
        public int AuthorId { get; private set; }
        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
