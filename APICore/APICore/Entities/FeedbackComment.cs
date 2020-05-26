#nullable enable
using System;

namespace APICore.Entities
{
    public class FeedbackComment
    {
        public int Id { get; private set; }
        public string? Content { get; private set; }
        public int UserId { get; private set; }
        public DateTime CreateAt { get; private set; }
        public int TourInfoId { get; private set; }
        public int AuthorId { get; private set; }
    }
}
