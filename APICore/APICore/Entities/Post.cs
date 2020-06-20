#nullable enable
using System;
using APICore.Models;

namespace APICore.Entities
{
    public class Post : IIsDeleted
    {
        public Post(string? content, string[]? images, int authorId, DateTime createAt, int? tourId, double? rating)
        {
            Content = content;
            Images = images;
            AuthorId = authorId;
            CreateAt = createAt;
            TourId = tourId;
            Rating = rating;
            TotalComment = 0;
            TotalLike = 0;
        }

        public Post()
        {
        }

        public Post Update(string? content, string[]? images, double? rating)
        {
            Content = content ?? Content;
            Images = images ?? Images;
            Rating = rating ?? rating;
            return this;
        }

        public int Id { get; private set; }
        public int? TourId { get; private set; }
        
        public double? Rating { get; private set; }
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