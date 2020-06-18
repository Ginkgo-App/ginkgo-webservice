#nullable enable
using System;
using APICore.Models;

namespace APICore.Entities
{
    public class Post : IIsDeleted
    {
        public Post(string? content, string[]? images, int authorId, DateTime at, int? tourId)
        {
            Content = content;
            Images = images;
            AuthorId = authorId;
            CreateAt = at;
            TourId = tourId;
            TotalComment = 0;
            TotalLike = 0;
        }

        public Post()
        {
        }

        public Post Update(string? content, string[]? images,  DateTime at)
        {
            Content = content ?? Content;
            Images = images ?? Images;
            return this;
        }

        public int Id { get; private set; }
        public int? TourId { get; private set; }
        public string? Content { get; private set; }
        public string[]? Images { get; private set; }
        public int TotalLike { get;  set; }
        public int TotalComment { get; set; }
        public int AuthorId { get; private set; }
        public DateTime CreateAt { get; private set; }
        public DateTime? DeletedAt { get; set; }

        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}