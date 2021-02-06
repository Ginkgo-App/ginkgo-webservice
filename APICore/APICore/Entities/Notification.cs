using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APICore.Entities
{
    public class Notification
    {
        public int Id { get; private set; }
        public int SenderId { get; private set; }
        public string Title { get; private set; }
        public string Message { get; private set; }
        public string? Style { get; private set; }
        public string? Payload { get; private set; }
        public DateTime? SendAt { get; private set; }
        public DateTime? SeenAt { get; private set; }
    }
}
