﻿//<auto-generated/>
#nullable enable
using System;
using Azure;
using Azure.Data.Tables;

namespace Devlooped
{
    /// <summary>
    /// Factory methods to create <see cref="ITableRepository{T}"/> instances
    /// that store entities as a serialized document.
    /// </summary>
    static partial class DocumentRepository
    {
        /// <summary>
        /// Creates an <see cref="ITableRepository{T}"/> for the given entity type 
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of entity that the repository will manage.</typeparam>
        /// <param name="storageAccount">The storage account to use.</param>
        /// <param name="tableName">Optional table name to use. If not provided, the <typeparamref name="T"/> 
        /// <c>Name</c> will be used, unless a <see cref="TableAttribute"/> on the type overrides it.</param>
        /// <param name="partitionKey">Optional function to retrieve the partition key for a given entity. 
        /// If not provided, the class will need a property annotated with <see cref="PartitionKeyAttribute"/>.</param>
        /// <param name="rowKey">Optional function to retrieve the row key for a given entity. 
        /// If not provided, the class will need a property annotated with <see cref="RowKeyAttribute"/>.</param>
        /// <param name="serializer">Optional serializer to use instead of the default <see cref="DocumentSerializer.Default"/>.</param>
        /// <returns>The new <see cref="ITableRepository{T}"/>.</returns>
        public static IDocumentRepository<T> Create<T>(
            CloudStorageAccount storageAccount,
            string? tableName = default,
            Func<T, string>? partitionKey = default,
            Func<T, string>? rowKey = default,
            IDocumentSerializer? serializer = default) where T : class
        {
            tableName ??= TableRepository.GetDefaultTableName<T>();
            partitionKey ??= PartitionKeyAttribute.CreateCompiledAccessor<T>();
            rowKey ??= RowKeyAttribute.CreateCompiledAccessor<T>();
            serializer ??= DocumentSerializer.Default;

            return new DocumentRepository<T>(storageAccount, tableName, partitionKey, rowKey, serializer);
        }
    }
}
