using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APICore.Entities
{
    public class UserGroup
    {
        public int ID { get; set; }
        public int UserId { get; set; }
        public int GroupId { get; set; }
    }
}
