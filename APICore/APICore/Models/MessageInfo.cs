using APICore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APICore.Models
{
    public class MessageInfo : Message
    {
        public SimpleUser Sender { get; set; }
        public GroupInfo Group { get; set; }
    }
}
