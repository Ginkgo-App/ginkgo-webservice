using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using APICore.Models;

namespace APICore.Entities
{
    public class Friend: IIsDeleted
    {
        public Friend(int userId, int requestedUserId, DateTime? acceptedAt = null)
        {
            UserId = userId;
            RequestedUserId = requestedUserId;
            AcceptedAt = acceptedAt;
            DeletedAt = null;
        }

        [Key]
        public int UserId { get; private set; }
        [Key]
        public int RequestedUserId { get; private set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
