using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Devlooped
{
    /// <summary>
    /// A generic repository that stores entities in table storage, using the properties 
    /// annotated with <see cref="PartitionKeyAttribute"/> and <see cref="RowKeyAttribute"/> 
    /// and optional <see cref="TableAttribute"/>.
    /// </summary>
    /// <typeparam name="T">The type of entity being persisted.</typeparam>
    /// <remarks>
    /// If no <see cref="TableAttribute"/> is provided, entities are persisted in a table 
    /// named after the <typeparamref name="T"/>, without the <c>Entity</c> word (if any).
    /// </remarks>
    partial interface ITableRepository<T> where T : class
    {
        /// <summary>
        /// Enumerates asynchronously all entities with the given <paramref name="partitionKey"/>.
        /// </summary>
        /// <param name="partitionKey">The partition key to scan and enumerate all entities from.</param>
        /// <param name="cancellation">Optional <see cref="CancellationToken"/>.</param>
        /// <returns></returns>
        IAsyncEnumerable<T> EnumerateAsync(string partitionKey, CancellationToken cancellation = default);

        /// <summary>
        /// Retrieves an entity from the repository.
        /// </summary>
        /// <param name="partitionKey">The entity partition key.</param>
        /// <param name="rowKey">The entity row key.</param>
        /// <param name="cancellation">Optional <see cref="CancellationToken"/>.</param>
        /// <returns>The retrieved entity, or <see langword="null"/> if not found.</returns>
        Task<T?> GetAsync(string partitionKey, string rowKey, CancellationToken cancellation = default);

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
        /// <param name="partitionKey">The entity partition key.</param>
        /// <param name="rowKey">The entity row key.</param>
        /// <param name="cancellation">Optional <see cref="CancellationToken"/>.</param>
        Task DeleteAsync(string partitionKey, string rowKey, CancellationToken cancellation = default);
    }
}
