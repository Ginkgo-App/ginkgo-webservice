#nullable enable
using System;
using System.ComponentModel.DataAnnotations.Schema;
using APICore.Models;
using Newtonsoft.Json.Linq;

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
        
        [NotMapped]
        public SimpleUser Author { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
        
        public JObject ToJson()
        {
            var result = JObject.FromObject(this);

            result.Remove("UserId");

            return result;
        }
    }
}
