#nullable enable
using System;

namespace APICore.Entities
{
    public class Post
    {
        public int Id { get; private set; }
        public string? Content { get; private set; }
        public string[]? Images { get; private set; }
        public int AuthorId { get; private set; }
        public DateTime CreateAt { get; private set; }
    }
}