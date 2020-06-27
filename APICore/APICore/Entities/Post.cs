#nullable enable
using System;
using System.ComponentModel.DataAnnotations.Schema;
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
        
        public double? Rating { get;  set; }
        public string? Content { get; private set; }
        public string[]? Images { get; private set; }
        public int TotalLike { get;  set; }
        public int TotalComment { get; set; }
        public int AuthorId { get; private set; }
        public DateTime CreateAt { get; private set; }
        public DateTime? DeletedAt { get; set; }
        [NotMapped]
        public PostComment? FeaturedComment { get; set; }
        [NotMapped]
        public SimpleUser? Author { get; set; }
        [NotMapped]
        public Tour? Tour { get; set; }
        [NotMapped]
        public bool? IsLike { get; set; }

        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}