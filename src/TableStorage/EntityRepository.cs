﻿//<auto-generated/>
#nullable enable
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace Devlooped
{
    /// <inheritdoc />
    partial class EntityRepository<T> : IEntityRepository<T> where T : class
    {
        static readonly string partitionKey = (typeof(T).Name.EndsWith("Entity") ? 
            typeof(T).Name.Substring(0, typeof(T).Name.Length - 6) : 
            typeof(T).Name);

        readonly ITableRepository<T> repository;

        /// <summary>
        /// Default table name to use when no value is provided via the constructor, which uses the 
        /// value defined in the <see cref="TableAttribute"/> applied to the <typeparamref name="T"/> 
        /// entity type, or <c>Entity</c> otherwise.
        /// </summary>
        public static string DefaultTableName { get; } = typeof(T).GetCustomAttribute<TableAttribute>()?.Name ?? "Entity";

        /// <summary>
        /// Initializes the repository with the given storage account and optional table name.
        /// </summary>
        /// <param name="storageAccount">The storage account to use.</param>
        /// <param name="tableName">Optional table name. If no value is provided, <see cref="DefaultTableName"/> will be used.</param>
        public EntityRepository(CloudStorageAccount storageAccount, string? tableName = default)
            => repository = new TableRepository<T>(storageAccount, tableName ?? default);

        /// <inheritdoc />
        public Task DeleteAsync(T entity, CancellationToken cancellation = default)
            => repository.DeleteAsync(entity, cancellation);

        /// <inheritdoc />
        public Task DeleteAsync(string rowKey, CancellationToken cancellation = default)
            => repository.DeleteAsync(partitionKey, rowKey, cancellation);
        
        /// <inheritdoc />
        public IAsyncEnumerable<T> EnumerateAsync(CancellationToken cancellation = default) 
            => repository.EnumerateAsync(partitionKey, cancellation);

        /// <inheritdoc />
        public Task<T?> GetAsync(string rowKey, CancellationToken cancellation = default)
            => repository.GetAsync(partitionKey, rowKey, cancellation);

        /// <inheritdoc />
        public Task<T> PutAsync(T entity, CancellationToken cancellation = default)
            => repository.PutAsync(entity, cancellation);
    }
}
