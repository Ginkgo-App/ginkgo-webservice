using System.ComponentModel.DataAnnotations;

namespace APICore.Entities
{
    public class TimelinePlace
    {
        [Key]
        public int PlaceId { get; set; }
        [Key]
        public int TimelineId { get; set; }
    }
}
