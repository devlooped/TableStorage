using System;
using System.Linq.Expressions;
using Azure.Data.Tables;

namespace Devlooped;

/// <summary>
/// Factory methods to create in-memory <see cref="ITableRepository{T}"/> and 
/// <see cref="IDocumentRepository{T}"/> instances (since <see cref="MemoryRepository{T}"/> 
/// implements both.
/// </summary>
public static class MemoryRepository
{
    /// <summary>
    /// Creates an <see cref="ITableRepository{TableEntity}"/> repository.
    /// </summary>
    /// <param name="tableName">Table name to use.</param>
    /// <returns>The new <see cref="ITableRepository{ITableEntity}"/>.</returns>
    public static MemoryRepository<TableEntity> Create(string tableName)
        => new(tableName, x => x.PartitionKey, x => x.RowKey);

    /// <summary>
    /// Creates an <see cref="ITableRepository{TableEntity}"/> repository.
    /// </summary>
    /// <returns>The new <see cref="ITableRepository{ITableEntity}"/>.</returns>
    public static MemoryRepository<TableEntity> Create()
        => new("Entities", x => x.PartitionKey, x => x.RowKey);

    /// <summary>
    /// Creates an <see cref="ITableRepository{T}"/> for the given entity type 
    /// <typeparamref name="T"/>, using the <typeparamref name="T"/> <c>Name</c> as 
    /// the table name.
    /// </summary>
    /// <typeparam name="T">The type of entity that the repository will manage.</typeparam>
    /// <param name="partitionKey">Function to retrieve the partition key for a given entity.</param>
    /// <param name="rowKey">Function to retrieve the row key for a given entity.</param>
    /// <returns>The new <see cref="ITableRepository{T}"/>.</returns>
    public static MemoryRepository<T> Create<T>(
        Expression<Func<T, string>> partitionKey,
        Expression<Func<T, string>> rowKey) where T : class
        => Create(typeof(T).Name, partitionKey, rowKey);

    /// <summary>
    /// Creates an <see cref="ITableRepository{T}"/> for the given entity type 
    /// <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of entity that the repository will manage.</typeparam>
    /// <param name="tableName">Optional table name to use. If not provided, the <typeparamref name="T"/> 
    /// <param name="partitionKey">Optional function to retrieve the partition key for a given entity. 
    /// If not provided, the class will need a property annotated with <see cref="PartitionKeyAttribute"/>.</param>
    /// <param name="rowKey">Optional function to retrieve the row key for a given entity. 
    /// If not provided, the class will need a property annotated with <see cref="RowKeyAttribute"/>.</param>
    /// <returns>The new <see cref="ITableRepository{T}"/>.</returns>
    public static MemoryRepository<T> Create<T>(
        string? tableName = default,
        Expression<Func<T, string>>? partitionKey = null,
        Expression<Func<T, string>>? rowKey = null) where T : class
    {
        partitionKey ??= PartitionKeyAttribute.CreateAccessor<T>();
        rowKey ??= RowKeyAttribute.CreateAccessor<T>();

        return new MemoryRepository<T>(tableName ?? TableRepository.GetDefaultTableName<T>(), partitionKey, rowKey);
    }
}
