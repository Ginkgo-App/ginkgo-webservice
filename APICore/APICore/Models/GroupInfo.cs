using APICore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APICore.Models
{
    public class GroupInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public DateTime LastUpdate { get; set; }
        public Message LastMessage { get; set; }
        public List<SimpleUser> Members { get; set; }
        public SimpleUser OtherUser { get; set; }
        public SimpleTour Tour { get; set; }
        public int TourId { get; set; }
        public int OtherUserId { get; set; }
        public int UnreadCount { get; set; }

        public GroupInfo(int id, string name, List<SimpleUser> members, string avatar, Message message)
        {
            Id = id;
            Name = name;
            Avatar = avatar;
            Members = members;
            Avatar = avatar;
            LastMessage = message;
        }

        public GroupInfo()
        {
        }
    }
}
