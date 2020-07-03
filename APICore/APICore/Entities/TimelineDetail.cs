using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        public int Id { get; private set; }
        public int PlaceId { get; private set; }
        public int TimelineId { get; set; }
        public string Time { get; private set; }
        public string Detail { get; private set; }
        [NotMapped]
        public Place Place { get; set; }

        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
