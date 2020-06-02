#nullable enable
using Newtonsoft.Json.Linq;
using System;
using APICore.Models;

namespace APICore.Entities
{
    public class TourInfo : IIsDeleted
    {
        public TourInfo(int createById, string name, string[] images, int startPlaceId, int destinatePlaceId)
        {
            CreateById = createById;
            Name = name;
            Images = images;
            StartPlaceId = startPlaceId;
            DestinatePlaceId = destinatePlaceId;
            DeleteAt = null;
        }

        public int Id { get; private set; }
        public int CreateById { get; private set; }
        public string? Name { get; private set; }
        public string[]? Images { get; private set; }
        public int StartPlaceId { get; private set; }
        public int DestinatePlaceId { get; private set; }
        public DateTime? DeleteAt { get; private set; }
        public double? Rating { get; private set; }

        public JObject ToJson(Place startPlace, Place destinatePlace )
        {
            JObject json = JObject.FromObject(this);

            json.Remove("StartPlaceId");
            json.Remove("DestinatePlaceId");
            json.Remove("CreateById");

            json.Add("StartPlace", startPlace == null ? null : JObject.FromObject(startPlace));
            json.Add("DestinatePlace", destinatePlace == null ? null : JObject.FromObject(destinatePlace));
            return json;
        }
        
        public TourInfo Update(int? createById = null, string? name = null, string[] images = null, int? startPlaceId = null, int? destinatePlaceId = null)
        {
            CreateById = createById ?? CreateById;
            Name = name ?? Name;
            Images = images ?? Images;
            StartPlaceId = startPlaceId ?? StartPlaceId;
            DestinatePlaceId = destinatePlaceId ?? DestinatePlaceId;
            return this;
        }

        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
