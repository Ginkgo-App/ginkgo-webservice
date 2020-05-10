using System;
using System.ComponentModel.DataAnnotations;

namespace APICore.Entities
{
    public class PostLike
    {
        [Key]
        public int UserId { get; set; }
        [Key]
        public int PostId { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
