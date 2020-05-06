using System;

namespace APICore.Entities
{
    public class TourMember
    {
        public int TourId { get; set; }
        public int UserId { get; set; }
        public bool IsHost { get; set; }
        public DateTime JoinAt { get; set; }
    }
}
