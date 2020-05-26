#nullable enable
using System;

namespace APICore.Entities
{
    public class TimeLine
    {
        public int Id { get; private set; }
        public DateTime Day { get; private set; }
        public string? Description { get; private set; }
        public int TourId { get; private set; }
    }
}
