﻿using System;
using System.Collections.Generic;
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
        static readonly JsonSerializer serializer = new JsonSerializer();

        static readonly Func<T, string> getPartitionKey = PartitionKeyAttribute.CreateAccessor<T>();
        static readonly Func<T, string> getRowKey = RowKeyAttribute.CreateAccessor<T>();

        static readonly PropertyInfo partitionKeyProp = typeof(T).GetProperties()
            .FirstOrDefault(prop => prop.GetCustomAttribute<PartitionKeyAttribute>() != null);
        static readonly PropertyInfo rowKeyProp = typeof(T).GetProperties()
            .First(prop => prop.GetCustomAttribute<RowKeyAttribute>() != null);

        readonly CloudStorageAccount storageAccount;
        readonly AsyncLazy<CloudTable> table;

        /// <summary>
        /// Default table name to use when no value is provided via the constructor, which uses the 
        /// value defined in the <see cref="TableAttribute"/> applied to the <typeparamref name="T"/> 
        /// entity type, or its <see cref="Type"/> name if not provided (minus the "Entity" suffix, 
        /// if present).
        /// </summary>
        public static string DefaultTableName { get; } = typeof(T).GetCustomAttribute<TableAttribute>()?.Name ??
            (typeof(T).Name.EndsWith("Entity") ? typeof(T).Name.Substring(0, typeof(T).Name.Length - 6) : typeof(T).Name);

        /// <summary>
        /// Initializes the repository with the given storage account and optional table name.
        /// </summary>
        /// <param name="storageAccount">The storage account to use.</param>
        /// <param name="tableName">Optional table name. If no value is provided, <see cref="DefaultTableName"/> will be used.</param>
        public TableRepository(CloudStorageAccount storageAccount, string? tableName = default)
        {
            this.storageAccount = storageAccount;
            table = new AsyncLazy<CloudTable>(() => GetTableAsync(tableName ?? DefaultTableName));
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
            var partitionKey = getPartitionKey.Invoke(entity);
            var rowKey = getRowKey.Invoke(entity);
            var properties = entity.GetType()
                .GetProperties()
                // Persist all properties except for the key properties, since those already have their own column
                .Where(prop => prop.GetCustomAttribute<PartitionKeyAttribute>() == null && prop.GetCustomAttribute<RowKeyAttribute>() == null)
                .ToDictionary(prop => prop.Name, prop => EntityProperty.CreateEntityPropertyFromObject(prop.GetValue(entity)));

            var table = await this.table.Value.ConfigureAwait(false);

            var result = await table.ExecuteAsync(TableOperation.InsertOrReplace(
                new DynamicTableEntity(partitionKey, rowKey, "*", properties)), cancellation)
                .ConfigureAwait(false);

            return ToEntity((DynamicTableEntity)result.Result);
        }

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
            var partitionKey = getPartitionKey.Invoke(entity);
            var rowKey = getRowKey.Invoke(entity);

            var table = await this.table.Value.ConfigureAwait(false);

            await table.ExecuteAsync(TableOperation.Delete(
                new DynamicTableEntity(partitionKey, rowKey) { ETag = "*" }), cancellation)
                .ConfigureAwait(false);
        }

        async Task<CloudTable> GetTableAsync(string tableName)
        {
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(string.IsNullOrEmpty(tableName) ? DefaultTableName : tableName);

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

            // Persist the key properties with the property name, so they can 
            // be resolved either via the ctor or as a property setter.
            if (partitionKeyProp != null)
            {
                writer.WritePropertyName(partitionKeyProp.Name);
                writer.WriteValue(entity.PartitionKey);
            }

            writer.WritePropertyName(rowKeyProp.Name);
            writer.WriteValue(entity.RowKey);

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
