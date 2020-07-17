﻿using System;
using System.ComponentModel.DataAnnotations;
using APICore.Models;

namespace APICore.Entities
{
    public class FeedbackLike : IIsDeleted
    {
        [Key]
        public int UserId { get; private set; }
        [Key]
        public int TourInfoId { get; private set; }
        [Key]
        public int AuthorId { get; private set; }
        public DateTime CreateAt { get; private set; }
        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
