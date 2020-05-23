#nullable enable
using System;

namespace APICore.Entities
{
    public class FeedbackComment
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public int UserId { get; set; }
        public DateTime CreateAt { get; set; }
        public int TourInfoId { get; set; }
        public int AuthorId { get; set; }
    }
}
