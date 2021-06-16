﻿//<auto-generated/>
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace Devlooped
{
    /// <inheritdoc />
    partial class TableEntityRepository : ITableRepository<TableEntity>
    {
        readonly CloudStorageAccount storageAccount;
        readonly Task<CloudTable> table;

        /// <summary>
        /// Initializes the table repository.
        /// </summary>
        /// <param name="storageAccount">The <see cref="CloudStorageAccount"/> to use to connect to the table.</param>
        /// <param name="tableName">The table that backs this repository.</param>
        protected internal TableEntityRepository(CloudStorageAccount storageAccount, string tableName)
        {
            this.storageAccount = storageAccount;
            TableName = tableName;
            table = GetTableAsync(TableName);
        }

        /// <inheritdoc />
        public string TableName { get; }

        /// <inheritdoc />
        public IQueryable<TableEntity> CreateQuery() 
            => storageAccount.CreateCloudTableClient().GetTableReference(TableName).CreateQuery<TableEntity>();

        /// <inheritdoc />
        public async Task DeleteAsync(string partitionKey, string rowKey, CancellationToken cancellation = default)
        {
            var table = await this.table.ConfigureAwait(false);

            await table.ExecuteAsync(TableOperation.Delete(
                new TableEntity(partitionKey, rowKey) { ETag = "*" }), cancellation)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(TableEntity entity, CancellationToken cancellation = default)
        {
            var table = await this.table.ConfigureAwait(false);
            entity.ETag = "*";
            await table.ExecuteAsync(TableOperation.Delete(entity), cancellation)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<TableEntity> EnumerateAsync(string? partitionKey = default, [EnumeratorCancellation] CancellationToken cancellation = default)
        {
            var table = await this.table;
            var query = new TableQuery<TableEntity>();
            if (partitionKey != null)
                query = query.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            TableContinuationToken? continuation = null;
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, continuation, cancellation)
                    .ConfigureAwait(false);

                foreach (var entity in segment)
                    if (entity != null)
                        yield return entity;

            } while (continuation != null && !cancellation.IsCancellationRequested);
        }

        /// <inheritdoc />
        public async Task<TableEntity?> GetAsync(string partitionKey, string rowKey, CancellationToken cancellation = default)
        {
            var table = await this.table.ConfigureAwait(false);
            var result = await table.ExecuteAsync(TableOperation.Retrieve(
                partitionKey, rowKey,
                (partitionKey, rowKey, timestamp, properties, etag) => new TableEntity(partitionKey, rowKey) { Timestamp = timestamp, ETag = etag }), 
                cancellation)
                .ConfigureAwait(false);

            return (TableEntity?)result.Result;
        }

        /// <inheritdoc />
        public async Task<TableEntity> PutAsync(TableEntity entity, CancellationToken cancellation = default)
        {
            var table = await this.table.ConfigureAwait(false);
            entity.ETag = "*";
            var result = await table.ExecuteAsync(TableOperation.InsertOrReplace(entity), cancellation)
                .ConfigureAwait(false);

            return (TableEntity)result.Result;
        }

        async Task<CloudTable> GetTableAsync(string tableName)
        {
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }
    }
}
