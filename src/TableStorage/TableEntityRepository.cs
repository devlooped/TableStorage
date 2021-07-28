﻿//<auto-generated/>
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace Devlooped
{
    /// <inheritdoc />
    partial class TableEntityRepository : ITableRepository<ITableEntity>
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

        /// <summary>
        /// The strategy to use when updating an existing entity.
        /// </summary>
        public UpdateStrategy UpdateStrategy { get; set; } = UpdateStrategy.Replace;

        /// <inheritdoc />
        public IQueryable<ITableEntity> CreateQuery() 
            => storageAccount.CreateCloudTableClient().GetTableReference(TableName).CreateQuery<DynamicTableEntity>();

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(string partitionKey, string rowKey, CancellationToken cancellation = default)
        {
            var table = await this.table.ConfigureAwait(false);

            try
            {
                var result = await table.ExecuteAsync(TableOperation.Delete(
                    new TableEntity(partitionKey, rowKey) { ETag = "*" }), cancellation)
                    .ConfigureAwait(false);

                return result.HttpStatusCode >= 200 && result.HttpStatusCode <= 299;
            }
            catch (StorageException)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync(ITableEntity entity, CancellationToken cancellation = default)
            => DeleteAsync(entity.PartitionKey, entity.RowKey, cancellation);

        /// <inheritdoc />
        public async IAsyncEnumerable<ITableEntity> EnumerateAsync(string? partitionKey = default, [EnumeratorCancellation] CancellationToken cancellation = default)
        {
            var table = await this.table;
            var query = new TableQuery<DynamicTableEntity>();
            if (partitionKey != null)
                query = query.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            TableContinuationToken? continuation = null;
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, continuation, cancellation)
                    .ConfigureAwait(false);

                continuation = segment.ContinuationToken;

                foreach (var entity in segment)
                    if (entity != null)
                        yield return entity;

            } while (continuation != null && !cancellation.IsCancellationRequested);
        }

        /// <inheritdoc />
        public async Task<ITableEntity?> GetAsync(string partitionKey, string rowKey, CancellationToken cancellation = default)
        {
            var table = await this.table.ConfigureAwait(false);
            var result = await table.ExecuteAsync(TableOperation.Retrieve(partitionKey, rowKey), cancellation)
                .ConfigureAwait(false);

            return (ITableEntity?)result.Result;
        }

        /// <inheritdoc />
        public async Task<ITableEntity> PutAsync(ITableEntity entity, CancellationToken cancellation = default)
        {
            var table = await this.table.ConfigureAwait(false);
            entity.ETag = "*";
            var result = await table.ExecuteAsync(UpdateStrategy.CreateOperation(entity), cancellation)
                .ConfigureAwait(false);

            if (UpdateStrategy == UpdateStrategy.Merge)
                return (await GetAsync(entity.PartitionKey, entity.RowKey, cancellation).ConfigureAwait(false))!;

            return (ITableEntity)result.Result;
        }

        Task<CloudTable> GetTableAsync(string tableName) => Task.Run(async () =>
        {
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        });
    }
}
