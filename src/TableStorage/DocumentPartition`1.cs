﻿//<auto-generated/>
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Devlooped
{
    /// <inheritdoc />
    partial class DocumentPartition<T> : IDocumentPartition<T> where T : class
    {
        readonly DocumentRepository<T> repository;

        /// <summary>
        /// Initializes the repository with the given storage account and optional table name.
        /// </summary>
        /// <param name="storageAccount">The <see cref="CloudStorageAccount"/> to use to connect to the table.</param>
        /// <param name="tableName">The table that backs this table partition.</param>
        /// <param name="partitionKey">The fixed partition key that backs this table partition.</param>
        /// <param name="rowKey">A function to determine the row key for an entity of type <typeparamref name="T"/> within the partition.</param>
        /// <param name="serializer">The serializer to use.</param>
        /// <param name="includeProperties">Whether to serialize properties as columns too, like table repositories, for easier querying.</param>
        protected internal DocumentPartition(CloudStorageAccount storageAccount, string tableName, string partitionKey, Func<T, string> rowKey, IDocumentSerializer serializer, bool includeProperties = false)
            : this(new TableConnection(storageAccount, tableName ?? DocumentPartition.GetDefaultTableName<T>()), partitionKey, rowKey, serializer, includeProperties)
        {
        }

        /// <summary>
        /// Initializes the repository with the given storage account and optional table name.
        /// </summary>
        /// <param name="tableConnection">The table to connect to.</param>
        /// <param name="partitionKey">The fixed partition key that backs this table partition.</param>
        /// <param name="rowKey">A function to determine the row key for an entity of type <typeparamref name="T"/> within the partition.</param>
        /// <param name="serializer">The serializer to use.</param>
        /// <param name="includeProperties">Whether to serialize properties as columns too, like table repositories, for easier querying.</param>
        protected internal DocumentPartition(TableConnection tableConnection, string partitionKey, Func<T, string> rowKey, IDocumentSerializer serializer, bool includeProperties = false)
        {
            PartitionKey = partitionKey ?? TablePartition.GetDefaultPartitionKey<T>();
            repository = new DocumentRepository<T>(
                tableConnection,
                _ => PartitionKey,
                rowKey ?? RowKeyAttribute.CreateCompiledAccessor<T>(),
                serializer, 
                includeProperties);
        }

        /// <inheritdoc />
        public string TableName => repository.TableName;

        /// <inheritdoc />
        public string PartitionKey { get; }

        /// <inheritdoc />
        public Task<bool> DeleteAsync(T entity, CancellationToken cancellation = default)
            => repository.DeleteAsync(entity, cancellation);

        /// <inheritdoc />
        public Task<bool> DeleteAsync(string rowKey, CancellationToken cancellation = default)
            => repository.DeleteAsync(PartitionKey, rowKey, cancellation);
        
        /// <inheritdoc />
        public IAsyncEnumerable<T> EnumerateAsync(CancellationToken cancellation = default) 
            => repository.EnumerateAsync(PartitionKey, cancellation);

        /// <inheritdoc />
        public IAsyncEnumerable<T> EnumerateAsync(Expression<Func<IDocumentEntity, bool>> predicate, CancellationToken cancellation = default)
            => repository.EnumerateAsync(e => e.PartitionKey == PartitionKey, cancellation);

        /// <inheritdoc />
        public Task<T?> GetAsync(string rowKey, CancellationToken cancellation = default)
            => repository.GetAsync(PartitionKey, rowKey, cancellation);

        /// <inheritdoc />
        public Task<T> PutAsync(T entity, CancellationToken cancellation = default)
            => repository.PutAsync(entity, cancellation);
    }
}
