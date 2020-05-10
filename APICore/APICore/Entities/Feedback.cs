using System;
using System.ComponentModel.DataAnnotations;

namespace APICore.Entities
{
    public class Feedback
    {
        [Key]
        public int AuthorId { get; set; }
        [Key]
        public int TourId { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
