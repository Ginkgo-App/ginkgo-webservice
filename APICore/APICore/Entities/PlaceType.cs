using System;
using APICore.Models;

#nullable enable
namespace APICore.Entities
{
    public class PlaceType : IIsDeleted
    {
        public PlaceType(string value, bool isHaveChild)
        {
            Value = value;
            IsHaveChild = isHaveChild;
        }

        public int Id { get; private set; }
        public string Value { get; private set; }
        public bool IsHaveChild { get; private set; }
        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}