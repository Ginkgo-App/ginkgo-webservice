using System.ComponentModel.DataAnnotations;

#nullable enable
namespace APICore.Entities
{
    public class ChildPlace
    {
        public ChildPlace(int parentId, int childId)
        {
            ParentId = parentId;
            ChildId = childId;
        }

        [Key]
        public int ParentId { get; private set; }
        [Key]
        public int ChildId { get; private set; }
    }
}
