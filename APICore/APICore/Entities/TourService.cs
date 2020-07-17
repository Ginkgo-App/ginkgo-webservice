using System;
using System.ComponentModel.DataAnnotations;
using APICore.Models;

namespace APICore.Entities
{
    public class TourService : IIsDeleted
    {
        public TourService(int serviceId, int tourId)
        {
            ServiceId = serviceId;
            TourId = tourId;
            DeletedAt = null;
        }

        [Key]
        public int ServiceId { get; private set; }
        [Key]
        public int TourId { get; private set; }

        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
