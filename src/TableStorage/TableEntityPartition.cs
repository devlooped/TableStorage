﻿//<auto-generated/>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace Devlooped
{
    /// <inheritdoc />
    partial class TableEntityPartition : ITablePartition<TableEntity>
    {
        readonly TableEntityRepository repository;

        /// <summary>
        /// Initializes the repository with the given storage account and optional table name.
        /// </summary>
        /// <param name="storageAccount">The <see cref="CloudStorageAccount"/> to use to connect to the table.</param>
        /// <param name="tableName">The table that backs this table partition.</param>
        /// <param name="partitionKey">The fixed partition key that backs this table partition.</param>
        protected internal TableEntityPartition(CloudStorageAccount storageAccount, string tableName, string partitionKey)
            : this(new TableConnection(storageAccount, tableName), partitionKey)
        {
        }

        /// <summary>
        /// Initializes the repository with the given storage account and optional table name.
        /// </summary>
        /// <param name="tableConnection">The <see cref="TableConnection"/> to use to connect to the table.</param>
        /// <param name="partitionKey">The fixed partition key that backs this table partition.</param>
        protected internal TableEntityPartition(TableConnection tableConnection, string partitionKey)
        {
            PartitionKey = partitionKey;
            repository = new TableEntityRepository(tableConnection);
        }

        /// <inheritdoc />
        public string TableName => repository.TableName;

        /// <inheritdoc />
        public string PartitionKey { get; }

        /// <summary>
        /// The <see cref="TableUpdateMode"/> to use when updating an existing entity.
        /// </summary>
        public TableUpdateMode UpdateMode { get; set; }

        /// <summary>
        /// The strategy to use when updating an existing entity.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public UpdateStrategy UpdateStrategy
        {
            // Backs-compatible implementation
            get => UpdateMode == TableUpdateMode.Replace ? UpdateStrategy.Replace : UpdateStrategy.Merge;
            set => UpdateMode = value.UpdateMode;
        }

        /// <inheritdoc />
        public IQueryable<TableEntity> CreateQuery() => repository.CreateQuery().Where(x => x.PartitionKey == PartitionKey);

        /// <inheritdoc />
        public Task<bool> DeleteAsync(TableEntity entity, CancellationToken cancellation = default)
        {
            if (!PartitionKey.Equals(entity.PartitionKey, StringComparison.Ordinal))
                throw new ArgumentException("Entity does not belong to the partition.");

            return repository.DeleteAsync(entity, cancellation);
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync(string rowKey, CancellationToken cancellation = default)
            => repository.DeleteAsync(PartitionKey, rowKey, cancellation);

        /// <inheritdoc />
        public IAsyncEnumerable<TableEntity> EnumerateAsync(CancellationToken cancellation = default) 
            => repository.EnumerateAsync(PartitionKey, cancellation);

        /// <inheritdoc />
        public Task<TableEntity?> GetAsync(string rowKey, CancellationToken cancellation = default)
            => repository.GetAsync(PartitionKey, rowKey, cancellation);

        /// <inheritdoc />
        public Task<TableEntity> PutAsync(TableEntity entity, CancellationToken cancellation = default)
        {
            if (!PartitionKey.Equals(entity.PartitionKey, StringComparison.Ordinal))
                throw new ArgumentException("Entity does not belong to the partition.");

            return repository.PutAsync(entity, cancellation);
        }
    }
}
