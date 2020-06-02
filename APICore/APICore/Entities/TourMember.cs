using System;
using System.ComponentModel.DataAnnotations;
using APICore.Models;

namespace APICore.Entities
{
    public class TourMember : IIsDeleted
    {
        [Key]
        public int TourId { get; private set; }
        [Key]
        public int UserId { get; private set; }
        public DateTime JoinAt { get; private set; }
        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
