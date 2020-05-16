using System;

namespace APICore.Entities
{
    public class Tour
    {
        public int id { get; set; }
        public string Name { get; set; }
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }
        public int MaxMember { get; set; }
        public int TourInfoId { get; set; }
    }
}