using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APICore.Entities
{
    public class Friend
    {
        public Friend(int userId, int requestedUserId, DateTime? acceptedAt = null)
        {
            UserId = userId;
            RequestedUserId = requestedUserId;
            AcceptedAt = acceptedAt;
        }

        [Key]
        public int UserId { get; private set; }
        [Key]
        public int RequestedUserId { get; private set; }
        public DateTime? AcceptedAt { get; set; }
    }
}
