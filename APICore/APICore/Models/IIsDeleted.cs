using System;

namespace APICore.Models
{
    public interface IIsDeleted
    {
        DateTime? DeletedAt { get; set; }

        public void Delete();
    }
}