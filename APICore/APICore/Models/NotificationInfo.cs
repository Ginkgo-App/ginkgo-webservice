using APICore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APICore.Models
{
    public class NotificationInfo : Notification
    {
        public string SenderName { get; set; }
    }
}
