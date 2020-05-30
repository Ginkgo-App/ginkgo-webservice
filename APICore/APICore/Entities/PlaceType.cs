#nullable enable
namespace APICore.Entities
{
    public class PlaceType
    {
        public PlaceType(string value)
        {
            Value = value;
        }

        public int Id { get; private set; }
        public string Value { get; private set; }
    }
}
