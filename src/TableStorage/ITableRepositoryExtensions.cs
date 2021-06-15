using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Devlooped
{
    /// <summary>
    /// Usability overloads for <see cref="ITableRepository{T}"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    static partial class ITableRepositoryExtensions
    {
        /// <summary>
        /// Queries the repository for items that match the given <paramref name="predicate"/>.
        /// </summary>
        /// <remarks>
        /// Shortcut for <c>CreateQuery().Where(predicate)</c> and returning as <see cref="IAsyncEnumerable{T}"/> 
        /// for use with <c>await foreach</c> directly.
        /// </remarks>
        /// <example>
        /// var books = TableRepository.Create&lt;Book&gt;();
        /// await foreach (var book in books.QueryAsync(x => x.IsPublished))
        /// {
        ///    Console.WriteLine(book.ISBN);
        /// }
        /// </example>
        public static IAsyncEnumerable<T> EnumerateAsync<T>(this ITableRepository<T> repository, Expression<Func<T, bool>> predicate, CancellationToken cancellation = default) where T : class
            => (IAsyncEnumerable<T>)repository.CreateQuery().Where(predicate);
    }
}
