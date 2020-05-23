namespace APICore.Entities
{
    public class Place
    {
        public Place(string name, string[] images, string description)
        {
            Name = name;
            Images = images;
            Description = description;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string[] Images { get; set; }
        public string Description { get; set; }
    }
}
