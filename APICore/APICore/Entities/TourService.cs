using System.ComponentModel.DataAnnotations;

namespace APICore.Entities
{
    public class TourService
    {
        [Key]
        public int ServiceId { get; set; }
        [Key]
        public int TourId { get; set; }
    }
}
