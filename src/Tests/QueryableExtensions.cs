using System.Collections.Generic;
using System.Linq;

namespace Devlooped
{
    public static class QueryableExtensions
    {
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IQueryable<T> queryable)
            => (IAsyncEnumerable<T>)queryable;
    }
}
