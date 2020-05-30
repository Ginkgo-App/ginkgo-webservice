using System.Collections.Generic;
using APICore.Entities;
using Newtonsoft.Json.Linq;

#nullable enable
namespace APICore.Models
{
    public class PlaceInfo
    {
        public PlaceInfo(Place place, PlaceType type, List<PlaceInfo> childPlaces)
        {
            Id = place.Id;
            Name = place.Name;
            Images = place.Images;
            Description = place.Description;
            Type = type;
            ChildPlaces = childPlaces;
        }

        public int Id { get; private set; }
        public string? Name { get; private set; }
        public string[]? Images { get; private set; }
        public string? Description { get; private set; }
        public PlaceType Type { get; private set; }
        public List<PlaceInfo> ChildPlaces { get; private set; }
        
        public JObject ToJson()
        {
            JObject json = JObject.FromObject(this);
            return json;
        }
    }
}