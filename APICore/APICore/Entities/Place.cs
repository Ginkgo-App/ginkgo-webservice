using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json.Linq;

#nullable enable
namespace APICore.Entities
{
    public class Place
    {
        public Place(int typeId, string? name, string[]? images, string? description)
        {
            TypeId = typeId;
            Name = name;
            Images = images;
            Description = description;
        }

        public int Id { get; private set; }
        public string? Name { get; private set; }
        public string[]? Images { get; private set; }
        public string? Description { get; private set; }
        public int TypeId { get; private set; }

        public Place Update(string? name, string[]? images, string? description, int? typeId)
        {
            Name = name ?? Name;
            Images = images ?? Images;
            Description = description ?? Description;
            TypeId = typeId ?? TypeId;
            return this;
        }
    }
}