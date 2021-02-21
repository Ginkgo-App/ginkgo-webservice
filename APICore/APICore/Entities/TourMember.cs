using System;
using System.ComponentModel.DataAnnotations;
using APICore.Models;
using Microsoft.AspNetCore.Mvc;

namespace APICore.Entities
{
    public class TourMember : IIsDeleted
    {
        public TourMember()
        {
        }

        public TourMember(int tourId, int userId)
        {
            TourId = tourId;
            UserId = userId;
            JoinAt = DateTime.Now; 
        }

        [Key]
        public int TourId { get; set; }
        [Key]
        public int UserId { get; set; }
        public DateTime JoinAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
