#nullable enable
using System;

namespace APICore.Entities
{
    public class PostComment
    {
        public PostComment(string? content, int userId, int postId, DateTime at)
        {
            Content = content;
            UserId = userId;
            PostId = postId;
            CreateAt = at;
        }

        public PostComment()
        {
        }

        public int Id { get; set; }
        public string? Content { get; set; }
        public int UserId { get; set; }
        public int PostId { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
