﻿//<auto-generated/>
#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Devlooped
{
    /// <summary>
    /// A generic repository that stores entities in table storage.
    /// </summary>
    /// <typeparam name="T">The type of entity being persisted.</typeparam>
    partial interface ITableStorage<T> where T : class
    {
        /// <summary>
        /// Gets the table name being used.
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// Deletes an entity from the repository.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <param name="cancellation">Optional <see cref="CancellationToken"/>.</param>
        /// <returns><see langword="true"/> if an existing record was deleted, <see langword="true"/> otherwise.</returns>
        Task<bool> DeleteAsync(T entity, CancellationToken cancellation = default);

        /// <summary>
        /// Deletes an entity from the repository given its <paramref name="partitionKey"/> and <paramref name="rowKey"/>.
        /// </summary>
        /// <param name="partitionKey">The entity partition key.</param>
        /// <param name="rowKey">The entity row key.</param>
        /// <param name="cancellation">Optional <see cref="CancellationToken"/>.</param>
        /// <returns><see langword="true"/> if an existing record was deleted, <see langword="true"/> otherwise.</returns>
        Task<bool> DeleteAsync(string partitionKey, string rowKey, CancellationToken cancellation = default);

        /// <summary>
        /// Enumerates asynchronously all entities, optionally within the given <paramref name="partitionKey"/>.
        /// </summary>
        /// <param name="partitionKey">The optional partition key to scope the enumeration to.</param>
        /// <param name="cancellation">Optional <see cref="CancellationToken"/>.</param>
        /// <returns></returns>
        IAsyncEnumerable<T> EnumerateAsync(string? partitionKey = default, CancellationToken cancellation = default);

        /// <summary>
        /// Retrieves an entity from the repository.
        /// </summary>
        /// <param name="partitionKey">The entity partition key.</param>
        /// <param name="rowKey">The entity row key.</param>
        /// <param name="cancellation">Optional <see cref="CancellationToken"/>.</param>
        /// <returns>The retrieved entity, or <see langword="null"/> if not found.</returns>
        Task<T?> GetAsync(string partitionKey, string rowKey, CancellationToken cancellation = default);

        /// <summary>
        /// Retrieves an entity from the repository.
        /// </summary>
        /// <param name="entity">The entity to use to lookup partition and row key values.</param>
        /// <param name="cancellation">Optional <see cref="CancellationToken"/>.</param>
        /// <returns>The retrieved entity, or <see langword="null"/> if not found.</returns>
        Task<T?> GetAsync(T entity, CancellationToken cancellation = default);

        /// <summary>
        /// Writes an entity to the table, overwriting an existing value, if any.
        /// </summary>
        /// <param name="entity">The entity to persist.</param>
        /// <param name="cancellation">Optional <see cref="CancellationToken"/>.</param>
        /// <returns>The saved entity.</returns>
        Task<T> PutAsync(T entity, CancellationToken cancellation = default);

        /// <summary>
        /// Writes a set of entities to the table, overwriting a existing values, if any.
        /// </summary>
        /// <param name="entities">The entities to persist.</param>
        /// <param name="cancellation">Optional <see cref="CancellationToken"/>.</param>
        /// <remarks>
        /// Automatically batches operations for better performance.
        /// </remarks>
        Task PutAsync(IEnumerable<T> entities, CancellationToken cancellation = default);
    }
}
