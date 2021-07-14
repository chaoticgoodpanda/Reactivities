using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace API.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, int currentPage, int itemsPerPage,
            int totalItems, int totalPages)
        {
            var paginationHeader = new
            {
                currentPage,
                itemsPerPage,
                totalItems,
                totalPages
            };
            //formatting as a JsonString with a key = paginationHeader
            response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationHeader));
            //need to explicitly expose this - returning Cors header
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
            
        }
    }
}