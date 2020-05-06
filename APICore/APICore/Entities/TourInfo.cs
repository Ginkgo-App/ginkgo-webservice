using System;

namespace APICore.Entities
{
    public class TourInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string[] Images { get; set; }
        public int StrartPlaceId { get; set; }
        public int DestinatePlaceId { get; set; }
        public DateTime DeleteAt { get; set; }
        public double Rating { get; set; }
    }
}
