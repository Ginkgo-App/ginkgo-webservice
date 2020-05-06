using System;

namespace APICore.Entities
{
    public class PostLike
    {
        public int UserId { get; set; }
        public int PostId { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
