using System;
using System.ComponentModel.DataAnnotations;

namespace APICore.Entities
{
    public class PostLike
    {
        [Key]
        public int UserId { get; private set; }
        [Key]
        public int PostId { get; private set; }
        public DateTime CreateAt { get; private set; }
    }
}
