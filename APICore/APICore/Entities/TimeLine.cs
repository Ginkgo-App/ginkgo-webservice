#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using APICore.Models;

namespace APICore.Entities
{
    public class TimeLine : IIsDeleted
    {
        public TimeLine(string day, string? description, List<TimelineDetail> timelineDetails)
        {
            Day = DateTime.Parse(day);
            Description = description;
            TimelineDetails = timelineDetails;
        }
        
        public TimeLine(DateTime day, string? description, List<TimelineDetail> timelineDetails)
        {
            Day = day;
            Description = description;
            TimelineDetails = timelineDetails;
        }

        public TimeLine()
        {
        }

        public int Id { get; private set; }
        public DateTime Day { get; set; }
        public string? Description { get; set; }
        public int TourId { get; set; }
        
        [NotMapped]
        public List<TimelineDetail> TimelineDetails { get; set; }
        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
