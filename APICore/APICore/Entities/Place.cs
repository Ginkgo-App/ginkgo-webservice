#nullable enable
namespace APICore.Entities
{
    public class Place
    {
        public Place(string? name, string[]? images, string? description)
        {
            Name = name;
            Images = images;
            Description = description;
        }

        public int Id { get; private set; }
        public string? Name { get; private set; }
        public string[]? Images { get; private set; }
        public string? Description { get; private set; }
        
        public Place Update(string? name, string[]? images, string? description)
        {
            Name = name ?? Name;
            Images = images ?? Images;
            Description = description ?? Description;
            return this;
        }
    }
}
