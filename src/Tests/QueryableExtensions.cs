using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Devlooped
{
    public static class QueryableExtensions
    {
        public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IQueryable<T> queryable)
        {
            if (queryable is IAsyncEnumerable<T> asyncEnumerable)
            {
                await foreach (var item in asyncEnumerable)
                {
                    yield return item;
                    await Task.Yield();
                }
                yield break;
            }

            foreach (var item in queryable)
            {
                yield return item;
                await Task.Yield();
            }
        }
    }
}
