using System;

namespace APICore.Entities
{
    public class Pagination
    {
        public Pagination(int totalElement, int currentPage, int pageSize)
        {
            TotalPage = Convert.ToInt32(Math.Ceiling((double)totalElement / pageSize)); ;
            TotalElement = totalElement;
            CurrentPage = currentPage;
            PageSize = pageSize;
        }

        public int TotalPage { get; set; }
        public int TotalElement { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}
