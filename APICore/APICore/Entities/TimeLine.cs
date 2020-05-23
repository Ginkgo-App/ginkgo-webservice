#nullable enable
using System;

namespace APICore.Entities
{
    public class TimeLine
    {
        public int Id { get; set; }
        public DateTime Day { get; set; }
        public string? Description { get; set; }
        public int TourId { get; set; }
    }
}
