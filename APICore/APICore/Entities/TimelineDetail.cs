using System;
using System.ComponentModel.DataAnnotations;
using APICore.Models;

namespace APICore.Entities
{
    public class TimelineDetail : IIsDeleted
    {
        public TimelineDetail(int placeId, string time, string detail)
        {
            PlaceId = placeId;
            Time = time;
            Detail = detail;
        }

        [Key]
        public int PlaceId { get; private set; }
        [Key]
        public int TimelineId { get; set; }
        public string Time { get; private set; }
        public string Detail { get; private set; }

        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
