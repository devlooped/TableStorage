﻿using System;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Azure.Cosmos.Table;

namespace Devlooped
{
    /// <summary>
    /// Factory methods to create <see cref="ITableRepository{T}"/> instances.
    /// </summary>
    static partial class TableRepository
    {
        static readonly ConcurrentDictionary<Type, string> defaultTableNames = new();

        /// <summary>
        /// Creates an <see cref="ITableRepository{T}"/> for the given entity type 
        /// <typeparamref name="T"/>, using the <typeparamref name="T"/> <c>Name</c> as 
        /// the table name.
        /// </summary>
        /// <typeparam name="T">The type of entity that the repository will manage.</typeparam>
        /// <param name="storageAccount">The storage account to use.</param>
        /// <param name="partitionKey">Function to retrieve the partition key for a given entity.</param>
        /// <param name="rowKey">Function to retrieve the row key for a given entity.</param>
        /// <returns>The new <see cref="ITableRepository{T}"/>.</returns>
        public static ITableRepository<T> Create<T>(
            CloudStorageAccount storageAccount,
            Func<T, string> partitionKey,
            Func<T, string> rowKey) where T : class
            => Create<T>(storageAccount, typeof(T).Name, partitionKey, rowKey);

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
        /// <returns>The new <see cref="ITableRepository{T}"/>.</returns>
        public static ITableRepository<T> Create<T>(
            CloudStorageAccount storageAccount,
            string? tableName = default,
            Func<T, string>? partitionKey = null,
            Func<T, string>? rowKey = null) where T : class
        {
            tableName ??= GetDefaultTableName<T>();
            partitionKey ??= PartitionKeyAttribute.CreateAccessor<T>();
            rowKey ??= RowKeyAttribute.CreateAccessor<T>();

            return new TableRepository<T>(storageAccount, tableName, partitionKey, rowKey);
        }

        /// <summary>
        /// Gets a default table name for entities of type <typeparamref name="T"/>. Will be the 
        /// <see cref="TableAttribute.Name"/> if the attribute is present, or the type name otherwise.
        /// </summary>
        public static string GetDefaultTableName<T>() =>
            defaultTableNames.GetOrAdd(typeof(T), type => type.GetCustomAttribute<TableAttribute>()?.Name ?? type.Name);
    }
}