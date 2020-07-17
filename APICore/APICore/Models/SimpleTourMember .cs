using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APICore.Entities;
using Newtonsoft.Json.Linq;

namespace APICore.Models
{
    public class SimpleTourMember
    {
        public int Id { get; }
        public string Name { get; }
        public string Avatar { get; }
        public string Job { get; }
        public string FriendType { get; }
        public DateTime JoinAt { get; }
        public DateTime? AcceptedAt { get; }

        public SimpleTourMember(int id, string name, string avatar, string job, string friendType, DateTime joinAt,
            DateTime? acceptedAt)
        {
            Id = id;
            Name = name;
            Avatar = avatar;
            Job = job;
            FriendType = friendType;
            JoinAt = joinAt;
            AcceptedAt = acceptedAt;
        }
    }
}