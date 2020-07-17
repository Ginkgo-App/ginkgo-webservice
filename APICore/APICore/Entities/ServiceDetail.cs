using System;
using APICore.Models;

namespace APICore.Entities
{
    public class ServiceDetail : IIsDeleted
    {
        public int Id { get; private set; }
        public int ServiceId { get; private set; }
        public int CreateById { get; private set; }
        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
