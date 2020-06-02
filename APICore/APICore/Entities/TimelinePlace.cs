using System;
using System.ComponentModel.DataAnnotations;
using APICore.Models;

namespace APICore.Entities
{
    public class TimelinePlace : IIsDeleted
    {
        [Key]
        public int PlaceId { get; private set; }
        [Key]
        public int TimelineId { get; private set; }

        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
