#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using APICore.Models;

namespace APICore.Entities
{
    public class Feedback : IIsDeleted
    {
        [Key]
        public int AuthorId { get; private set; }
        [Key]
        public int TourId { get; private set; }
        public string? Content { get; private set; }
        public int? Rating { get; private set; }
        public DateTime CreateAt { get; private set; }
        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
