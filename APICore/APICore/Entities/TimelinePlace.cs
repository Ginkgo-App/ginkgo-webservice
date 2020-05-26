using System.ComponentModel.DataAnnotations;

namespace APICore.Entities
{
    public class TimelinePlace
    {
        [Key]
        public int PlaceId { get; private set; }
        [Key]
        public int TimelineId { get; private set; }
    }
}
