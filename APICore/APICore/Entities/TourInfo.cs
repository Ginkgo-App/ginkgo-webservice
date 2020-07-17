#nullable enable
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
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
            DeletedAt = null;
        }

        public int Id { get; private set; }
        public int CreateById { get; private set; }
        public string? Name { get; private set; }
        public string[]? Images { get; private set; }
        public int StartPlaceId { get; private set; }
        public int DestinatePlaceId { get; private set; }
        public int TotalRating { get; set; }
        public double? Rating { get; set; }

        [NotMapped] public SimpleUser CreateBy { get; set; }
        [NotMapped] public Place StartPlace { get; set; }
        [NotMapped] public Place DestinatePlace { get; set; }
        public DateTime? DeletedAt { get; set; }

        public JObject ToJson(SimpleUser? createBy = null, Place? startPlace = null, Place? destinatePlace = null)
        {
            CreateBy = createBy ?? CreateBy;
            StartPlace = startPlace ?? StartPlace;
            DestinatePlace = destinatePlace ?? DestinatePlace;

            JObject json = JObject.FromObject(this);

            json.Remove("StartPlaceId");
            json.Remove("CreateById");
            json.Remove("DestinatePlaceId");
            return json;
        }

        public TourInfo Update(int? createById = null, string? name = null, string[] images = null,
            int? startPlaceId = null, int? destinatePlaceId = null)
        {
            CreateById = createById ?? CreateById;
            Name = name ?? Name;
            Images = images ?? Images;
            StartPlaceId = startPlaceId ?? StartPlaceId;
            DestinatePlaceId = destinatePlaceId ?? DestinatePlaceId;
            return this;
        }

        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}