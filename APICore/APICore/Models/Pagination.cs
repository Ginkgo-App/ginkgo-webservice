using System;
using System.Globalization;

namespace APICore.Models
{
    public class Pagination
    {
        public Pagination(int totalElement, int currentPage, int pageSize)
        {
            // if (int.TryParse(
            //     Math.Ceiling((double) totalElement / (pageSize <= 0 ? Vars.DefaultPageSize : pageSize))
            //         .ToString(CultureInfo.CurrentCulture),
            //     out var totalPage))
            // {
            //     TotalPage = totalPage;
            // }

            TotalPage = Convert.ToInt32(Math
                .Ceiling((double) totalElement / (pageSize <= 0 ? Vars.DefaultPageSize : pageSize))
                .ToString(CultureInfo.CurrentCulture));

            TotalElement = totalElement;
            CurrentPage = currentPage;
            PageSize = pageSize;
        }

        public int TotalPage { get; }
        public int TotalElement { get; }
        public int CurrentPage { get; }
        public int PageSize { get; }
    }
}