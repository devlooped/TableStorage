﻿//<auto-generated/>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;

namespace Devlooped
{
    /// <inheritdoc />
    partial class TableRepository<T> : ITableRepository<T> where T : class
    {
        static readonly IStringDocumentSerializer serializer = DocumentSerializer.Default;

        readonly TableConnection tableConnection;
        readonly Func<T, string> partitionKey;
        readonly string? partitionKeyProperty;
        readonly Func<T, string> rowKey;
        readonly string? rowKeyProperty;

        /// <summary>
        /// Initializes the table repository.
        /// </summary>
        /// <param name="storageAccount">The <see cref="CloudStorageAccount"/> to use to connect to the table.</param>
        public TableRepository(CloudStorageAccount storageAccount)
            : this(storageAccount,
                TableRepository.GetDefaultTableName<T>(),
                PartitionKeyAttribute.CreateAccessor<T>(),
                RowKeyAttribute.CreateAccessor<T>()) 
        { }

        /// <summary>
        /// Initializes the table repository.
        /// </summary>
        /// <param name="storageAccount">The <see cref="CloudStorageAccount"/> to use to connect to the table.</param>
        /// <param name="tableName">The table that backs this repository.</param>
        public TableRepository(CloudStorageAccount storageAccount, string tableName)
            : this(storageAccount, tableName ?? TableRepository.GetDefaultTableName<T>(),
                PartitionKeyAttribute.CreateAccessor<T>(),
                RowKeyAttribute.CreateAccessor<T>()) 
        { }

        /// <summary>
        /// Initializes the table repository.
        /// </summary>
        /// <param name="tableConnection">The <see cref="TableConnection"/> to use to connect to the table.</param>
        public TableRepository(TableConnection tableConnection)
            : this(tableConnection,
                PartitionKeyAttribute.CreateAccessor<T>(),
                RowKeyAttribute.CreateAccessor<T>())
        { }

        /// <summary>
        /// Initializes the table repository.
        /// </summary>
        /// <param name="storageAccount">The <see cref="CloudStorageAccount"/> to use to connect to the table.</param>
        /// <param name="tableName">The table that backs this repository.</param>
        /// <param name="partitionKey">A function to determine the partition key for an entity of type <typeparamref name="T"/>.</param>
        /// <param name="rowKey">A function to determine the row key for an entity of type <typeparamref name="T"/>.</param>
        public TableRepository(CloudStorageAccount storageAccount, string tableName, Expression<Func<T, string>>? partitionKey, Expression<Func<T, string>>? rowKey)
            : this(new TableConnection(storageAccount, tableName ?? TableRepository.GetDefaultTableName<T>()), 
                  partitionKey, rowKey)
        { }

        /// <summary>
        /// Initializes the table repository.
        /// </summary>
        /// <param name="tableConnection">The <see cref="TableConnection"/> to use to connect to the table.</param>
        /// <param name="partitionKey">A function to determine the partition key for an entity of type <typeparamref name="T"/>.</param>
        /// <param name="rowKey">A function to determine the row key for an entity of type <typeparamref name="T"/>.</param>
        public TableRepository(TableConnection tableConnection, Expression<Func<T, string>>? partitionKey, Expression<Func<T, string>>? rowKey)
        {
            this.tableConnection = tableConnection; 
            this.partitionKey = partitionKey == null ?
                PartitionKeyAttribute.CreateCompiledAccessor<T>() :
                partitionKey.Compile();

            partitionKeyProperty = partitionKey.GetPropertyName();

            this.rowKey = rowKey == null ?
                RowKeyAttribute.CreateCompiledAccessor<T>() :
                rowKey.Compile();

            rowKeyProperty = rowKey.GetPropertyName();
        }

        /// <inheritdoc />
        public string TableName => tableConnection.TableName;

        /// <summary>
        /// The <see cref="TableUpdateMode"/> to use when updating an existing entity.
        /// </summary>
        public TableUpdateMode UpdateMode { get; set; }

        /// <summary>
        /// Whether to persist the entity properties marked as partition and row 
        /// keys with their original property names too or not. Defaults to false.
        /// </summary>
        public bool PersistKeyProperties { get; set; }

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
        public IQueryable<T> CreateQuery() => new TableRepositoryQuery<T>(tableConnection.StorageAccount, serializer, tableConnection.TableName, partitionKeyProperty, rowKeyProperty);

        /// <inheritdoc />
        public IAsyncEnumerable<T> QueryAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellation = default)
            => (IAsyncEnumerable<T>)CreateQuery().Where(predicate);

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(string partitionKey, string rowKey, CancellationToken cancellation = default)
        {
            var table = await this.tableConnection.GetTableAsync().ConfigureAwait(false);
            var result = await table.DeleteEntityAsync(partitionKey, rowKey, cancellationToken: cancellation)
                .ConfigureAwait(false);

            return !result.IsError;
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync(T entity, CancellationToken cancellation = default)
            => DeleteAsync(partitionKey(entity), rowKey(entity), cancellation);

        /// <inheritdoc />
        public async IAsyncEnumerable<T> EnumerateAsync(string? partitionKey = default, [EnumeratorCancellation] CancellationToken cancellation = default)
        {
            var table = await this.tableConnection.GetTableAsync().ConfigureAwait(false);
            var filter = default(string);
            if (partitionKey != null)
                filter = "PartitionKey eq '" + partitionKey + "'";

            await foreach (var entity in table.QueryAsync<TableEntity>(filter, cancellationToken: cancellation).WithCancellation(cancellation))
            {
                yield return EntityPropertiesMapper.Default.ToEntity<T>(entity, partitionKeyProperty, rowKeyProperty);
            }
        }

        /// <inheritdoc />
        public Task<T?> GetAsync(T entity, CancellationToken cancellation = default)
            => GetAsync(partitionKey(entity), rowKey(entity), cancellation);

        /// <inheritdoc />
        public async Task<T?> GetAsync(string partitionKey, string rowKey, CancellationToken cancellation = default)
        {
            var table = await this.tableConnection.GetTableAsync().ConfigureAwait(false);

            try
            {
                var result = await table.GetEntityAsync<TableEntity>(partitionKey, rowKey, cancellationToken: cancellation)
                    .ConfigureAwait(false);

                return EntityPropertiesMapper.Default.ToEntity<T>(result.Value, partitionKeyProperty, rowKeyProperty);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return default;
            }
        }

        /// <inheritdoc />
        public async Task<T> PutAsync(T entity, CancellationToken cancellation = default)
        {
            var partitionKey = this.partitionKey.Invoke(entity);
            var rowKey = this.rowKey.Invoke(entity);
            var values = PersistKeyProperties ?
                EntityPropertiesMapper.Default.ToProperties(entity) : 
                EntityPropertiesMapper.Default.ToProperties(entity, partitionKeyProperty, rowKeyProperty);

            values[nameof(ITableEntity.PartitionKey)] = partitionKey;
            values[nameof(ITableEntity.RowKey)] = rowKey;

            var table = await this.tableConnection.GetTableAsync().ConfigureAwait(false);
            var result = await table.UpsertEntityAsync(new TableEntity(values), UpdateMode, cancellation)
                .ConfigureAwait(false);
            
            return (await GetAsync(partitionKey, rowKey, cancellation).ConfigureAwait(false))!;
        }
    }
}
