using Newtonsoft.Json.Linq;
using System;

namespace APICore.Entities
{
    public class TourInfo
    {
        public TourInfo(int createById, string name, string[] images, int startPlaceId, int destinatePlaceId)
        {
            CreateById = createById;
            Name = name;
            Images = images;
            StartPlaceId = startPlaceId;
            DestinatePlaceId = destinatePlaceId;
        }

        public int Id { get; set; }
        public int CreateById { get; set; }
        public string Name { get; set; }
        public string[] Images { get; set; }
        public int StartPlaceId { get; set; }
        public int DestinatePlaceId { get; set; }
        public DateTime DeleteAt { get; set; }
        public double Rating { get; set; }

        public JObject ToJson(Place startPlace, Place destinatePlace)
        {
            JObject json = JObject.FromObject(this);

            json.Remove("StartPlaceId");
            json.Remove("DestinatePlaceId");
            json.Remove("CreateById");

            json.Add("StartPlace", startPlace == null ? null : JObject.FromObject(startPlace));
            json.Add("DestinatePlace", destinatePlace == null ? null : JObject.FromObject(destinatePlace));
            return json;
        }
    }
}
