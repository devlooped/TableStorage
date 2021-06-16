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
        /// <summary>
        /// Queries the document repository for items that match the given <paramref name="predicate"/>.
        /// </summary>
        /// <example>
        /// var books = DocumentRepository.Create&lt;Book&gt;();
        /// await foreach (var book in books.EnumerateAsync(x => x.PartitionKey == "Rick Riordan" && x.DocumentType  ))
        /// {
        ///    Console.WriteLine(book.ISBN);
        /// }
        /// </example>
        public IAsyncEnumerable<T> EnumerateAsync(Expression<Func<IDocumentEntity, bool>> predicate, CancellationToken cancellation = default);
    }
}