using System;

namespace APICore.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TourId { get; set; }
        public string Content { get; set; }
        public string[] Images { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
