﻿//<auto-generated/>
#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Devlooped
{
    /// <summary>
    /// A generic repository that stores entities stores the entity type as 
    /// the partition key, and therefore only requires <see cref="RowKeyAttribute"/> 
    /// applied to the entity identifier property. If <see cref="TableAttribute"/> 
    /// is not provided, it defaults to <c>Entity</c>.
    /// </summary>
    /// <typeparam name="T">The type of entity being persisted.</typeparam>
    /// <remarks>
    /// If no <see cref="TableAttribute"/> is provided, all entities are persisted 
    /// in the same table (<c>Entity</c> by default), since the partition key is used 
    /// to differentiate by entity type.
    /// </remarks>
    partial interface IEntityRepository<T> where T : class
    {
        /// <summary>
        /// Enumerates asynchronously all entities of the same type.
        /// </summary>
        /// <param name="cancellation">Optional <see cref="CancellationToken"/>.</param>
        /// <returns></returns>
        IAsyncEnumerable<T> EnumerateAsync(CancellationToken cancellation = default);

        /// <summary>
        /// Retrieves an entity from the repository.
        /// </summary>
        /// <param name="rowKey">The entity row key.</param>
        /// <param name="cancellation">Optional <see cref="CancellationToken"/>.</param>
        /// <returns>The retrieved entity, or <see langword="null"/> if not found.</returns>
        Task<T?> GetAsync(string rowKey, CancellationToken cancellation = default);

        /// <summary>
        /// Writes an entity to the table, overwriting an existing value, if any.
        /// </summary>
        /// <param name="entity">The entity to persist.</param>
        /// <param name="cancellation">Optional <see cref="CancellationToken"/>.</param>
        /// <returns>The saved entity.</returns>
        Task<T> PutAsync(T entity, CancellationToken cancellation = default);

        /// <summary>
        /// Deletes an entity from the repository.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <param name="cancellation">Optional <see cref="CancellationToken"/>.</param>
        Task DeleteAsync(T entity, CancellationToken cancellation = default);

        /// <summary>
        /// Deletes an entity from the repository given its <paramref name="partitionKey"/> and <paramref name="rowKey"/>.
        /// </summary>
        /// <param name="rowKey">The entity row key.</param>
        /// <param name="cancellation">Optional <see cref="CancellationToken"/>.</param>
        Task DeleteAsync(string rowKey, CancellationToken cancellation = default);
    }
}