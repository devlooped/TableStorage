﻿//<auto-generated/>
#nullable enable
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Azure.Cosmos.Table;

namespace Devlooped
{
    /// <summary>
    /// Factory methods to create <see cref="ITablePartition{T}"/> instances 
    /// that store entities using individual columns for entity properties.
    /// </summary>
    static partial class TablePartition
    {
        static readonly ConcurrentDictionary<Type, string> defaultTableNames = new();

        /// <summary>
        /// Default table name to use when a value is not not provided 
        /// (or overriden via <see cref="TableAttribute"/>), which is <c>Entity</c>.
        /// </summary>
        public const string DefaultTableName = "Entities";

        /// <summary>
        /// Creates an <see cref="ITablePartition{ITableEntity}"/>, using 
        /// <see cref="DefaultTableName"/> as the table name and the 
        /// <typeparamref name="T"/> <c>Name</c> as the partition key.
        /// </summary>
        /// <param name="storageAccount">The storage account to use.</param>
        /// <param name="updateStrategy">Strategy to apply when updating an existing entity. Defaults to <see cref="UpdateStrategy.Replace"/>.</param>
        /// <returns>The new <see cref="ITablePartition{TEntity}"/>.</returns>
        public static ITablePartition<ITableEntity> Create(CloudStorageAccount storageAccount, string tableName, string partitionKey, UpdateStrategy? updateStrategy = default)
            => new TableEntityPartition(storageAccount, tableName, partitionKey) { UpdateStrategy = updateStrategy ?? UpdateStrategy.Replace };

        /// <summary>
        /// Creates an <see cref="ITablePartition{T}"/> for the given entity type 
        /// <typeparamref name="T"/>, using <see cref="DefaultTableName"/> as the table name and the 
        /// <typeparamref name="T"/> <c>Name</c> as the partition key.
        /// </summary>
        /// <typeparam name="T">The type of entity that the repository will manage.</typeparam>
        /// <param name="storageAccount">The storage account to use.</param>
        /// <param name="rowKey">Function to retrieve the row key for a given entity.</param>
        /// <returns>The new <see cref="ITablePartition{T}"/>.</returns>
        public static ITablePartition<T> Create<T>(
            CloudStorageAccount storageAccount,
            Expression<Func<T, string>> rowKey) where T : class
            => Create<T>(storageAccount, DefaultTableName, default, rowKey);

        /// <summary>
        /// Creates an <see cref="ITablePartition{T}"/> for the given entity type 
        /// <typeparamref name="T"/>, using the given table name and the 
        /// <typeparamref name="T"/> <c>Name</c> as the partition key.
        /// </summary>
        /// <typeparam name="T">The type of entity that the repository will manage.</typeparam>
        /// <param name="storageAccount">The storage account to use.</param>
        /// <param name="tableName">Table name to use.</param>
        /// <param name="rowKey">Function to retrieve the row key for a given entity.</param>
        /// <returns>The new <see cref="ITablePartition{T}"/>.</returns>
        public static ITablePartition<T> Create<T>(
            CloudStorageAccount storageAccount,
            string tableName,
            Expression<Func<T, string>> rowKey) where T : class
            => Create<T>(storageAccount, tableName, default, rowKey);

        /// <summary>
        /// Creates an <see cref="ITablePartition{T}"/> for the given entity type 
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of entity that the repository will manage.</typeparam>
        /// <param name="storageAccount">The storage account to use.</param>
        /// <param name="tableName">Optional table name to use. If not provided, <see cref="DefaultTableName"/> 
        /// will be used, unless a <see cref="TableAttribute"/> on the type overrides it.</param>
        /// <param name="partitionKey">Optional fixed partition key to scope entity persistence. 
        /// If not provided, the <typeparamref name="T"/> <c>Name</c> will be used.</param>
        /// <param name="rowKey">Optional function to retrieve the row key for a given entity. 
        /// If not provided, the class will need a property annotated with <see cref="RowKeyAttribute"/>.</param>
        /// <param name="updateStrategy">Strategy to apply when updating an existing entity. Defaults to <see cref="UpdateStrategy.Replace"/>.</param>
        /// <returns>The new <see cref="ITablePartition{T}"/>.</returns>
        public static ITablePartition<T> Create<T>(
            CloudStorageAccount storageAccount,
            string? tableName = default,
            string? partitionKey = null,
            Expression<Func<T, string>>? rowKey = null,
            UpdateStrategy? updateStrategy = default) where T : class
        {
            tableName ??= GetDefaultTableName<T>();
            partitionKey ??= GetDefaultPartitionKey<T>();
            rowKey ??= RowKeyAttribute.CreateAccessor<T>();

            return new TablePartition<T>(storageAccount, tableName, partitionKey, rowKey)
            {
                UpdateStrategy = updateStrategy ?? UpdateStrategy.Replace
            };
        }

        /// <summary>
        /// Gets a default table name for entities of type <typeparamref name="T"/>. Will be the 
        /// <see cref="TableAttribute.Name"/> if the attribute is present, or <see cref="DefaultTableName"/> otherwise.
        /// </summary>
        public static string GetDefaultTableName<T>(string fallbackName = DefaultTableName) =>
            defaultTableNames.GetOrAdd(typeof(T), type => type.GetCustomAttribute<TableAttribute>()?.Name ?? DefaultTableName);

        /// <summary>
        /// Gets a default partition key to use for entities of type <typeparamref name="T"/>. Will be the 
        /// the type name, stripped of a suffix <c>Entity</c> if present.
        /// </summary>
        public static string GetDefaultPartitionKey<T>() => typeof(T).Name.EndsWith("Entity") ?
            typeof(T).Name.Substring(0, typeof(T).Name.Length - 6) :
            typeof(T).Name;
    }
}