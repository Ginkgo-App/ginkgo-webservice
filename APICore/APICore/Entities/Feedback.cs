#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace APICore.Entities
{
    public class Feedback
    {
        [Key]
        public int AuthorId { get; private set; }
        [Key]
        public int TourId { get; private set; }
        public string? Content { get; private set; }
        public int? Rating { get; private set; }
        public DateTime CreateAt { get; private set; }
    }
}
