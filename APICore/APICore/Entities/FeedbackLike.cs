using System;
using System.ComponentModel.DataAnnotations;

namespace APICore.Entities
{
    public class FeedbackLike
    {
        [Key]
        public int UserId { get; set; }
        [Key]
        public int TourInfoId { get; set; }
        [Key]
        public int AuthorId { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
