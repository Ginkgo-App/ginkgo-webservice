using APICore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APICore.Models
{
    public class GroupInfo
    {
        public int Id { get; }
        public string Name { get; }
        public string Avatar { get; }
        public DateTime LastUpdate { get; }
        public Message LastMessage { get; }
        public List<SimpleUser> Members { get; }

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
