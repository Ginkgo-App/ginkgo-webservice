using System;
using System.ComponentModel.DataAnnotations;

namespace APICore.Entities
{
    public class TourMember
    {
        [Key]
        public int TourId { get; set; }
        [Key]
        public int UserId { get; set; }
        public bool IsHost { get; set; }
        public DateTime JoinAt { get; set; }
    }
}
