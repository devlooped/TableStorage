﻿//<auto-generated/>
#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace Devlooped
{
    /// <inheritdoc />
    partial class TableRepository<T> : ITableRepository<T> where T : class
    {
        static readonly ConcurrentDictionary<Type, PropertyInfo[]> entityProperties = new();

        static readonly JsonSerializer serializer = new JsonSerializer();

        readonly CloudStorageAccount storageAccount;
        readonly Func<T, string> partitionKey;
        readonly Func<T, string> rowKey;
        readonly AsyncLazy<CloudTable> table;

        /// <summary>
        /// Initializes the table repository.
        /// </summary>
        /// <param name="storageAccount">The <see cref="CloudStorageAccount"/> to use to connect to the table.</param>
        /// <param name="tableName">The table that backs this repository.</param>
        /// <param name="partitionKey">A function to determine the partition key for an entity of type <typeparamref name="T"/>.</param>
        /// <param name="rowKey">A function to determine the row key for an entity of type <typeparamref name="T"/>.</param>
        protected internal TableRepository(CloudStorageAccount storageAccount, string tableName, Func<T, string> partitionKey, Func<T, string> rowKey)
        {
            this.storageAccount = storageAccount;
            TableName = tableName ?? TableRepository.GetDefaultTableName<T>();
            this.partitionKey = partitionKey ?? PartitionKeyAttribute.CreateAccessor<T>();
            this.rowKey = rowKey ?? RowKeyAttribute.CreateAccessor<T>();
            table = new AsyncLazy<CloudTable>(() => GetTableAsync(TableName));
        }

        /// <inheritdoc />
        public string TableName { get; }

        /// <inheritdoc />
        public async Task DeleteAsync(string partitionKey, string rowKey, CancellationToken cancellation = default)
        {
            var table = await this.table.Value.ConfigureAwait(false);

            await table.ExecuteAsync(TableOperation.Delete(
                new DynamicTableEntity(partitionKey, rowKey) { ETag = "*" }), cancellation)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(T entity, CancellationToken cancellation = default)
        {
            var partitionKey = this.partitionKey.Invoke(entity);
            var rowKey = this.rowKey.Invoke(entity);
            var table = await this.table.Value.ConfigureAwait(false);

            await table.ExecuteAsync(TableOperation.Delete(
                new DynamicTableEntity(partitionKey, rowKey) { ETag = "*" }), cancellation)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<T> EnumerateAsync(string partitionKey, [EnumeratorCancellation] CancellationToken cancellation = default)
        {
            var table = await this.table.Value;
            var query = new TableQuery<DynamicTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            TableContinuationToken? continuation = null;
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, continuation, cancellation)
                    .ConfigureAwait(false);

                foreach (var entity in segment)
                    if (entity != null)
                        yield return ToEntity(entity);

            } while (continuation != null && !cancellation.IsCancellationRequested);
        }

        /// <inheritdoc />
        public async Task<T?> GetAsync(string partitionKey, string rowKey, CancellationToken cancellation = default)
        {
            var table = await this.table.Value.ConfigureAwait(false);
            var result = await table.ExecuteAsync(TableOperation.Retrieve(partitionKey, rowKey), cancellation)
                .ConfigureAwait(false);

            if (result?.Result == null)
                return default;

            return ToEntity((DynamicTableEntity)result.Result);
        }

        /// <inheritdoc />
        public async Task<T> PutAsync(T entity, CancellationToken cancellation = default)
        {
            var partitionKey = this.partitionKey.Invoke(entity);
            var rowKey = this.rowKey.Invoke(entity);
            var properties = entityProperties.GetOrAdd(entity.GetType(), type => type
                .GetProperties()
                .Where(prop => prop.GetCustomAttribute<BrowsableAttribute>()?.Browsable != false)
                .ToArray());

            var table = await this.table.Value.ConfigureAwait(false);
            var values = properties
                .ToDictionary(prop => prop.Name, prop => EntityProperty.CreateEntityPropertyFromObject(prop.GetValue(entity)));

            var result = await table.ExecuteAsync(TableOperation.InsertOrReplace(
                new DynamicTableEntity(partitionKey, rowKey, "*", values)), cancellation)
                .ConfigureAwait(false);

            return ToEntity((DynamicTableEntity)result.Result);
        }

        async Task<CloudTable> GetTableAsync(string tableName)
        {
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }

        /// <summary>
        /// Uses JSON deserialization to convert from the persisted entity data 
        /// to the entity type, so that the right constructor and property 
        /// setters can be invoked, even if they are internal/private.
        /// </summary>
        T ToEntity(DynamicTableEntity entity)
        {
            using var json = new StringWriter();
            using var writer = new JsonTextWriter(json);

            // Write entity properties in json format so deserializer can 
            // perform its advanced ctor and conversion detection as usual.
            writer.WriteStartObject();
            foreach (var property in entity.Properties)
            {
                writer.WritePropertyName(property.Key);
                writer.WriteValue(property.Value.PropertyAsObject);
            }
            writer.WriteEndObject();

            using var reader = new JsonTextReader(new StringReader(json.ToString()));
            return serializer.Deserialize<T>(reader)!;
        }
    }
}
