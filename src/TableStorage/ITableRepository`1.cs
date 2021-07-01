﻿//<auto-generated/>
#nullable enable
using System.Linq;

namespace Devlooped
{
    /// <summary>
    /// A specialized <see cref="ITableRepository{T}"/> which allows querying 
    /// the repository by the entity properties, since they are stored in individual 
    /// columns.
    /// </summary>
    /// <typeparam name="T">The type of entity being persisted.</typeparam>
    partial interface ITableRepository<T> : ITableStorage<T> where T : class
    {
        /// <summary>
        /// Creates a query for use with LINQ expressions. See 
        /// <see ref="https://docs.microsoft.com/en-us/rest/api/storageservices/query-operators-supported-for-the-table-service">supported operators</see>.
        /// </summary>
        /// <example>
        /// var books = TableRepository.Create&lt;Book&gt;();
        /// await foreach (var book in books.CreateQuery().Where(x => x.IsPublished))
        /// {
        ///    Console.WriteLine(book.ISBN);
        /// }
        /// </example>
        /// <example>
        /// var books = TableRepository.Create&lt;Book&gt;();
        /// await foreach (var published in from book in books.CreateQuery()
        ///                                 where book.IsPublished && book.Pages > 1000
        ///                                 select book)
        /// {
        ///    Console.WriteLine(published.ISBN);
        /// }
        /// </example>
        IQueryable<T> CreateQuery();
    }
}