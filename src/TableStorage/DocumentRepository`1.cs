﻿//<auto-generated/>
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace Devlooped
{
    /// <inheritdoc />
    partial class DocumentRepository<T> : IDocumentRepository<T> where T : class
    {
        static readonly string documentVersion;
        static readonly int documentMajorVersion;
        static readonly int documentMinorVersion;

        readonly CloudStorageAccount storageAccount;

        readonly IStringDocumentSerializer? stringSerializer;
        readonly IBinaryDocumentSerializer? binarySerializer;

        readonly Func<T, string> partitionKey;
        readonly Func<T, string> rowKey;
        readonly Task<CloudTable> table;

        readonly Func<Expression<Func<IDocumentEntity, bool>>?, CancellationToken, IAsyncEnumerable<T>> enumerate;
        readonly Func<string, string, CancellationToken, Task<T?>> get;
        readonly Func<T, CancellationToken, Task<T>> put;

        static DocumentRepository()
        {
            var version = (typeof(T).Assembly.GetName().Version ?? new Version(1, 0));
            documentVersion = version.ToString(2);
            documentMajorVersion = version.Major;
            documentMinorVersion = version.Minor;
        }

        /// <summary>
        /// Initializes the table repository.
        /// </summary>
        /// <param name="storageAccount">The <see cref="CloudStorageAccount"/> to use to connect to the table.</param>
        /// <param name="tableName">The table that backs this repository.</param>
        /// <param name="partitionKey">A function to determine the partition key for an entity of type <typeparamref name="T"/>.</param>
        /// <param name="rowKey">A function to determine the row key for an entity of type <typeparamref name="T"/>.</param>
        protected internal DocumentRepository(CloudStorageAccount storageAccount, string tableName, Func<T, string> partitionKey, Func<T, string> rowKey, IDocumentSerializer serializer)
        {
            this.storageAccount = storageAccount;
            TableName = tableName ?? TableRepository.GetDefaultTableName<T>();

            this.partitionKey = partitionKey ?? PartitionKeyAttribute.CreateCompiledAccessor<T>();
            this.rowKey = rowKey ?? RowKeyAttribute.CreateCompiledAccessor<T>();

            stringSerializer = serializer as IStringDocumentSerializer;
            binarySerializer = serializer as IBinaryDocumentSerializer;

            if (stringSerializer == null && binarySerializer == null)
                throw new ArgumentException($"A valid serializer implementing either {nameof(IBinaryDocumentSerializer)} or {nameof(IStringDocumentSerializer)} is required.", nameof(serializer));

            // Use the right strategy depending on the provided serializer.
            if (stringSerializer != null)
            {
                enumerate = EnumerateStringAsync;
                get = GetStringAsync;
                put = PutStringAsync;
            }
            else
            {
                enumerate = EnumerateBinaryAsync;
                get = GetBinaryAsync;
                put = PutBinaryAsync;
            }

            table = GetTableAsync(TableName);
        }

        /// <inheritdoc />
        public string TableName { get; }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(string partitionKey, string rowKey, CancellationToken cancellation = default)
        {
            var table = await this.table.ConfigureAwait(false);

            try
            {
                var result = await table.ExecuteAsync(TableOperation.Delete(
                    new TableEntity(partitionKey, rowKey) { ETag = "*" }), cancellation);

                return result.HttpStatusCode >= 200 && result.HttpStatusCode <= 299;
            }
            catch (StorageException)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync(T entity, CancellationToken cancellation = default)
            => DeleteAsync(partitionKey(entity), rowKey(entity), cancellation);

        /// <inheritdoc />
        public IAsyncEnumerable<T> EnumerateAsync(string? partitionKey = default, CancellationToken cancellation = default)
            => enumerate(partitionKey == null ? null : e => e.PartitionKey == partitionKey, cancellation);

        /// <inheritdoc />
        public IAsyncEnumerable<T> EnumerateAsync(Expression<Func<IDocumentEntity, bool>> predicate, CancellationToken cancellation = default)
            => enumerate(predicate, cancellation);

        /// <inheritdoc />
        public Task<T?> GetAsync(string partitionKey, string rowKey, CancellationToken cancellation = default)
            => get(partitionKey, rowKey, cancellation);

        /// <inheritdoc />
        public Task<T> PutAsync(T entity, CancellationToken cancellation = default)
            => put(entity, cancellation);

        #region Binary

        async IAsyncEnumerable<T> EnumerateBinaryAsync(Expression<Func<IDocumentEntity, bool>>? predicate, [EnumeratorCancellation] CancellationToken cancellation = default)
        {
            var table = await this.table.ConfigureAwait(false);
            var query = table.CreateQuery<BinaryDocumentEntity>();

            if (predicate != null)
            {
                var expression = Expression.Lambda<Func<BinaryDocumentEntity, bool>>(predicate.Body, Expression.Parameter(typeof(BinaryDocumentEntity)));
                query = (TableQuery<BinaryDocumentEntity>)((IQueryable<BinaryDocumentEntity>)query).Where(expression);
            }

            TableContinuationToken? continuation = null;
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, continuation, cancellation)
                    .ConfigureAwait(false);

                foreach (var entity in segment)
                {
                    if (entity != null && entity.Document != null)
                    {
                        var value = binarySerializer!.Deserialize<T>(entity.Document);
                        if (value != null)
                            yield return value;
                    }
                }

            } while (continuation != null && !cancellation.IsCancellationRequested);
        }

        async Task<T?> GetBinaryAsync(string partitionKey, string rowKey, CancellationToken cancellation = default)
        {
            var table = await this.table.ConfigureAwait(false);
            var result = await table.ExecuteAsync(TableOperation.Retrieve<BinaryDocumentEntity>(
                partitionKey, rowKey),
                cancellation)
                .ConfigureAwait(false);

            var document = (BinaryDocumentEntity?)result.Result;
            if (document?.Document == null)
                return default;

            return binarySerializer!.Deserialize<T>(document.Document);
        }

        async Task<T> PutBinaryAsync(T entity, CancellationToken cancellation = default)
        {
            var partitionKey = this.partitionKey.Invoke(entity);
            var rowKey = this.rowKey.Invoke(entity);
            var table = await this.table.ConfigureAwait(false);

            // We use Replace because all the existing entity data is in a single 
            // column, no point in merging since it can't be done at that level anyway.
            var result = await table.ExecuteAsync(TableOperation.InsertOrReplace(
                new BinaryDocumentEntity(partitionKey, rowKey)
                {
                    ETag = "*",
                    Document = binarySerializer!.Serialize(entity),
                    Type = typeof(T).FullName,
                    Version = documentVersion,
                    MajorVersion = documentMajorVersion,
                    MinorVersion = documentMinorVersion,
                }), cancellation)
                .ConfigureAwait(false);

            var document = (BinaryDocumentEntity)result.Result;
            if (document.Document == null)
                return entity;

            return binarySerializer.Deserialize<T>(document.Document) ?? entity;
        }

        #endregion

        #region String

        async IAsyncEnumerable<T> EnumerateStringAsync(Expression<Func<IDocumentEntity, bool>>? predicate, [EnumeratorCancellation] CancellationToken cancellation = default)
        {
            var table = await this.table.ConfigureAwait(false);
            var query = table.CreateQuery<DocumentEntity>();

            if (predicate != null)
            {
                var expression = Expression.Lambda<Func<DocumentEntity, bool>>(predicate.Body, Expression.Parameter(typeof(DocumentEntity)));
                query = (TableQuery<DocumentEntity>)((IQueryable<DocumentEntity>)query).Where(expression);
            }

            TableContinuationToken? continuation = null;
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, continuation, cancellation)
                    .ConfigureAwait(false);

                foreach (var entity in segment)
                {
                    if (entity != null && entity.Document != null)
                    {
                        var value = stringSerializer!.Deserialize<T>(entity.Document);
                        if (value != null)
                            yield return value;
                    }
                }

            } while (continuation != null && !cancellation.IsCancellationRequested);
        }

        async Task<T?> GetStringAsync(string partitionKey, string rowKey, CancellationToken cancellation = default)
        {
            var table = await this.table.ConfigureAwait(false);
            var result = await table.ExecuteAsync(TableOperation.Retrieve<DocumentEntity>(
                partitionKey, rowKey),
                cancellation)
                .ConfigureAwait(false);

            var document = (DocumentEntity?)result.Result;
            if (document?.Document == null)
                return default;

            return stringSerializer!.Deserialize<T>(document.Document);
        }

        async Task<T> PutStringAsync(T entity, CancellationToken cancellation = default)
        {
            var partitionKey = this.partitionKey.Invoke(entity);
            var rowKey = this.rowKey.Invoke(entity);
            var table = await this.table.ConfigureAwait(false);

            // We use Replace because all the existing entity data is in a single 
            // column, no point in merging since it can't be done at that level anyway.
            var result = await table.ExecuteAsync(TableOperation.InsertOrReplace(
                new DocumentEntity(partitionKey, rowKey)
                {
                    ETag = "*",
                    Document = stringSerializer!.Serialize(entity),
                    Type = typeof(T).FullName,
                    Version = documentVersion,
                    MajorVersion = documentMajorVersion,
                    MinorVersion = documentMinorVersion,
                }), cancellation)
                .ConfigureAwait(false);

            var document = (DocumentEntity)result.Result;
            if (document.Document == null)
                return entity;

            return stringSerializer.Deserialize<T>(document.Document) ?? entity;
        }

        #endregion

        async Task<CloudTable> GetTableAsync(string tableName)
        {
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }

        class BinaryDocumentEntity : TableEntity, IDocumentEntity
        {
            public BinaryDocumentEntity() { }
            public BinaryDocumentEntity(string partitionKey, string rowKey) : base(partitionKey, rowKey) { }
            public byte[]? Document { get; set; }
            public string? Type { get; set; }
            public string? Version { get; set; }
            public int? MajorVersion { get; set; }
            public int? MinorVersion { get; set; }
        }

        class DocumentEntity : TableEntity, IDocumentEntity
        {
            public DocumentEntity() { }
            public DocumentEntity(string partitionKey, string rowKey) : base(partitionKey, rowKey) { }
            public string? Document { get; set; }
            public string? Type { get; set; }
            public string? Version { get; set; }
            public int? MajorVersion { get; set; }
            public int? MinorVersion { get; set; }
        }
    }
}