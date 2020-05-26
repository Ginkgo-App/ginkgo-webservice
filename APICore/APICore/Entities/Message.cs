#nullable enable
using System;

namespace APICore.Entities
{
    public class Message
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public int TourId { get; private set; }
        public string? Content { get; private set; }
        public string[]? Images { get; private set; }
        public DateTime CreateAt { get; private set; }
    }
}
