﻿//<auto-generated/>
#nullable enable
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        /// <param name="tableName">The table that backs this repository.</param>
        /// <param name="partitionKey">A function to determine the partition key for an entity of type <typeparamref name="T"/>.</param>
        /// <param name="rowKey">A function to determine the row key for an entity of type <typeparamref name="T"/>.</param>
        protected internal TableRepository(CloudStorageAccount storageAccount, string tableName, Expression<Func<T, string>>? partitionKey, Expression<Func<T, string>>? rowKey)
            : this(new TableConnection(storageAccount, tableName ?? TableRepository.GetDefaultTableName<T>()), partitionKey, rowKey)
        {
        }

        /// <summary>
        /// Initializes the table repository.
        /// </summary>
        /// <param name="tableConnection">The <see cref="TableConnection"/> to use to connect to the table.</param>
        /// <param name="partitionKey">A function to determine the partition key for an entity of type <typeparamref name="T"/>.</param>
        /// <param name="rowKey">A function to determine the row key for an entity of type <typeparamref name="T"/>.</param>
        protected internal TableRepository(TableConnection tableConnection, Expression<Func<T, string>>? partitionKey, Expression<Func<T, string>>? rowKey)
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
                yield return ToEntity(entity);
            }
        }

        /// <inheritdoc />
        public async Task<T?> GetAsync(string partitionKey, string rowKey, CancellationToken cancellation = default)
        {
            var table = await this.tableConnection.GetTableAsync().ConfigureAwait(false);

            try
            {
                var result = await table.GetEntityAsync<TableEntity>(partitionKey, rowKey, cancellationToken: cancellation)
                    .ConfigureAwait(false);

                return ToEntity(result.Value);
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
            var values = EntityPropertiesMapper.Default.ToProperties(entity, partitionKeyProperty, rowKeyProperty);

            values[nameof(ITableEntity.PartitionKey)] = partitionKey;
            values[nameof(ITableEntity.RowKey)] = rowKey;

            var table = await this.tableConnection.GetTableAsync().ConfigureAwait(false);
            var result = await table.UpsertEntityAsync(new TableEntity(values), UpdateMode, cancellation)
                .ConfigureAwait(false);
            
            return (await GetAsync(partitionKey, rowKey, cancellation).ConfigureAwait(false))!;
        }

        /// <summary>
        /// Uses JSON deserialization to convert from the persisted entity data 
        /// to the entity type, so that the right constructor and property 
        /// setters can be invoked, even if they are internal/private.
        /// </summary>
        T ToEntity(TableEntity entity)
        {
            using var mem = new MemoryStream();
            using var writer = new Utf8JsonWriter(mem);

            // Write entity properties in json format so deserializer can 
            // perform its advanced ctor and conversion detection as usual.
            writer.WriteStartObject();

            if (partitionKeyProperty != null && !entity.ContainsKey(partitionKeyProperty))
                writer.WriteString(partitionKeyProperty, entity.PartitionKey);

            if (rowKeyProperty != null && !entity.ContainsKey(rowKeyProperty))
                writer.WriteString(rowKeyProperty, entity.RowKey);

            if (entity.Timestamp != null && !entity.ContainsKey(nameof(ITableEntity.Timestamp)))
                writer.WriteString(nameof(ITableEntity.Timestamp), entity.Timestamp.Value.ToString("O"));

            foreach (var property in entity)
            {
                switch (property.Value)
                {
                    case string value:
                        writer.WriteString(property.Key, value);
                        break;
                    case byte[] value:
                        writer.WriteBase64String(property.Key, value);
                        break;
                    case bool value:
                        writer.WriteBoolean(property.Key, value);
                        break;
                    case DateTime value:
                        writer.WriteString(property.Key, value);
                        break;
                    case DateTimeOffset value:
                        writer.WriteString(property.Key, value);
                        break;
                    case double value:
                        writer.WriteNumber(property.Key, value);
                        break;
                    case int value:
                        writer.WriteNumber(property.Key, value);
                        break;
                    case long value:
                        writer.WriteNumber(property.Key, value);
                        break;
                    case Guid value:
                        writer.WriteString(property.Key, value);
                        break;
                    default:
                        break;
                }
            }

            writer.WriteEndObject();
            writer.Flush();
            mem.Position = 0;

            var json = new StreamReader(mem).ReadToEnd();

            return serializer.Deserialize<T>(json)!;
        }
    }
}
