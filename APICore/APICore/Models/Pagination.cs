namespace APICore.Entities
{
    public class Pagination
    {
        public int TotalPage { get; set; }
        public int TotalElement { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}
