using System;

namespace APICore.Entities
{
    public class Feedback
    {
        public int AuthorId { get; set; }
        public int TourInfoId { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
