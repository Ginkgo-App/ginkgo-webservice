using System;
using System.ComponentModel.DataAnnotations;

namespace APICore.Entities
{
    public class TourMember
    {
        [Key]
        public int TourId { get; private set; }
        [Key]
        public int UserId { get; private set; }
        public DateTime JoinAt { get; private set; }
    }
}
