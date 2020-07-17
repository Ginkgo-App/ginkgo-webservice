using System;
using System.ComponentModel.DataAnnotations;
using APICore.Models;

namespace APICore.Entities
{
    public class PostLike : IIsDeleted
    {
        public PostLike(int userId, int postId)
        {
            UserId = userId;
            PostId = postId;
            CreateAt = DateTime.Now;
        }

        public PostLike()
        {
        }

        [Key]
        public int UserId { get; private set; }
        [Key]
        public int PostId { get; private set; }
        public DateTime CreateAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
