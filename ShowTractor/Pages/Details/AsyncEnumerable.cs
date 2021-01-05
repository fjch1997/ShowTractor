using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShowTractor.Pages.Details
{
    public static class AsyncEnumerable
    {
        public async static IAsyncEnumerable<T> FromQueryable<T>(IQueryable<T> queryable)
        {
            var value = await queryable.ToArrayAsync();
            foreach (var item in value)
            {
                yield return item;
            }
        }
        public async static IAsyncEnumerable<T> FromEnumerable<T>(Task<IEnumerable<T>> enumerable)
        {
            var value = await enumerable;
            foreach (var item in value)
            {
                yield return item;
            }
        }
    }
}
