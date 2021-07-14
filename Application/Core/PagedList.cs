using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Application.Core
{
    //PagedList inherits everything in List class and we'll extend PagedList to give it some pagination properties
    public class PagedList<T> : List<T>
    {
        public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            PageSize = pageSize;
            TotalCount = count;
            AddRange(items);
        }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            //source is list of items (query) that is going to our DB. We want to get the count of items before any pagination has taken place.
            //so we'll know number of items in list. This is a query before pagination has taken place.
            var count = await source.CountAsync();
            
            //e.g. list of 12 items and have page size of 10. In order to get the first 10 records need the pageNumber -1 (=0) divided by page size,
            //which gives us zero. Next is page 2 - 1 = 1, * pageSize (10), which gives us the next 10 records.
            //finally query goes to DB with ToListAsync().
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedList<T>(items, count, pageNumber, pageSize); 
        } 
    }
}