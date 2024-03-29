﻿//<auto-generated/>
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;

namespace Devlooped
{
    /// <summary>
    /// A generic repository that stores entities in table storage as serialized 
    /// documents.
    /// </summary>
    /// <typeparam name="T">The type of entity being persisted.</typeparam>
    partial interface IDocumentRepository<T> : ITableStorage<T> where T : class
    {
        ///// <summary>
        ///// Creates a query for use with LINQ expressions. See 
        ///// <see ref="https://docs.microsoft.com/en-us/rest/api/storageservices/query-operators-supported-for-the-table-service">supported operators</see>.
        ///// </summary>
        ///// <example>
        ///// var books = DocumentRepository.Create&lt;Book&gt;();
        ///// await foreach (var book in books.CreateQuery().Where(x => x.PartitionKey == "Rick Riordan" &amp;&amp; x.Version == "1.2"))
        ///// {
        /////    Console.WriteLine(book.ISBN);
        ///// }
        ///// </example>
        ///// <example>
        ///// var books = TableRepository.Create&lt;Book&gt;();
        ///// await foreach (var published in from book in books.CreateQuery()
        /////                                 where book.IsPublished &amp;&amp; book.Pages > 1000
        /////                                 select book)
        ///// {
        /////    Console.WriteLine(published.ISBN);
        ///// }
        ///// </example>
        //IQueryable<T> CreateQuery();

        /// <summary>
        /// Queries the document repository for items that match the given <paramref name="predicate"/>.
        /// </summary>
        /// <example>
        /// var books = DocumentRepository.Create&lt;Book&gt;();
        /// await foreach (var book in books.EnumerateAsync(x => x.PartitionKey == "Rick Riordan" &amp;&amp; x.DocumentType  ))
        /// {
        ///    Console.WriteLine(book.ISBN);
        /// }
        /// </example>
        public IAsyncEnumerable<T> EnumerateAsync(Expression<Func<IDocumentEntity, bool>> predicate, CancellationToken cancellation = default);
    }
}