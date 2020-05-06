using System;

namespace APICore.Entities
{
    public class FeedbackLike
    {
        public int UserId { get; set; }
        public int TourInfoId { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
