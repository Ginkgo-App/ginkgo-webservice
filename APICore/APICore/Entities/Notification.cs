using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APICore.Entities
{
    public class Notification
    {
        public int Id { get; private set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string? Type { get; set; }
        public string? Payload { get; set; }
        public DateTime? SendAt { get; set; }
        public DateTime? SeenAt { get; set; }
    }
}
