#nullable enable
using System;
using APICore.Models;

namespace APICore.Entities
{
    public class PostComment : IIsDeleted
    {
        public PostComment(string? content, int userId, int postId, DateTime at)
        {
            Content = content;
            UserId = userId;
            PostId = postId;
            CreateAt = at;
            DeletedAt = null;
        }

        public PostComment()
        {
        }

        public int Id { get; private set; }
        public string? Content { get; private set; }
        public int UserId { get; private set; }
        public int PostId { get; private set; }
        public DateTime CreateAt { get; private set; }
        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
