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

        public int Id { get; private set; }
        public string? Content { get; private set; }
        public int UserId { get; private set; }
        public int PostId { get; private set; }
        public DateTime CreateAt { get; private set; }
    }
}
