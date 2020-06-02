using System;
using System.ComponentModel.DataAnnotations;
using APICore.Models;

#nullable enable
namespace APICore.Entities
{
    public class ChildPlace : IIsDeleted
    {
        public ChildPlace(int parentId, int childId)
        {
            ParentId = parentId;
            ChildId = childId;
            DeletedAt = null;
        }

        [Key]
        public int ParentId { get; private set; }
        [Key]
        public int ChildId { get; private set; }

        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}
