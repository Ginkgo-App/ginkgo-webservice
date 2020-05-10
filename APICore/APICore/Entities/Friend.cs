using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APICore.Entities
{
    public class Friend
    {
        [Key]
        public int UserId { get; set; }
        [Key]
        public int UserOtherId { get; set; }
    }
}
