using System;

namespace WebMvcPluginUser.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string[] Images { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreateAt { get; set; }
    }
}