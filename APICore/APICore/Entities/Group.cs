using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APICore.Entities
{
    public class Group
    {
        public int ID { get; set; }
        public int CreatorId { get; set; }
        public string GroupName { get; set; }
        public string Avatar { get; set; }
        public int TourId { get; set; }
        public int LastMessageId { get; set; }
        public DateTime LastMessageAt { get; set; }
        public DateTime LastUpdate { get; set; }
    };
}
