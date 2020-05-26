using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APICore.Entities
{
    public class Friend
    {
        public Friend(int userId, int requestedUserId, bool isAccepted = false)
        {
            UserId = userId;
            RequestedUserId = requestedUserId;
            IsAccepted = isAccepted;
        }

        [Key]
        public int UserId { get; private set; }
        [Key]
        public int RequestedUserId { get; private set; }
        public bool IsAccepted { get; set; }
    }
}
