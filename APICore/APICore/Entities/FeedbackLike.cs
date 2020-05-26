using System;
using System.ComponentModel.DataAnnotations;

namespace APICore.Entities
{
    public class FeedbackLike
    {
        [Key]
        public int UserId { get; private set; }
        [Key]
        public int TourInfoId { get; private set; }
        [Key]
        public int AuthorId { get; private set; }
        public DateTime CreateAt { get; private set; }
    }
}
