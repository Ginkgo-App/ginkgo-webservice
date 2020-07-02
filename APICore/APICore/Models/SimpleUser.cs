using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APICore.Models
{
    public class SimpleUser
    {
        public int Id { get; }
        public string Name { get; }
        public string Avatar { get; }
        public string Job { get; }
        public string FriendType { get; }
        public int TotalPost { get; }

        public SimpleUser(int id, string name, string avatar, string job, string friendType, int totalPost)
        {
            Id = id;
            Name = name;
            Avatar = avatar;
            Job = job;
            FriendType = friendType;
            TotalPost = totalPost;
        }
    }
}
