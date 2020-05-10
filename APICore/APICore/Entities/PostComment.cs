using System;

namespace APICore.Entities
{
    public class PostComment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int UserId { get; set; }
        public int PostId { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
