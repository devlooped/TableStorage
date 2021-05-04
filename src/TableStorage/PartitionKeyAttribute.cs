﻿using System;

namespace Devlooped
{
    /// <summary>
    /// Flags the property to use as the table storage partition key 
    /// when storing the annotated type using the <see cref="TableRepository{T}"/>.
    /// Can be applied at the class level instead with a fixed value to persist 
    /// entities with a fixed partition key value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    partial class PartitionKeyAttribute : TableStorageAttribute
    {
        /// <summary>
        /// Used to annotate a property that will be used as the partition key.
        /// </summary>
        public PartitionKeyAttribute() { }

        /// <summary>
        /// Used to annotate the class with a fixed partition key value to be 
        /// used for all entities.
        /// </summary>
        public PartitionKeyAttribute(string partitionKey) => PartitionKey = partitionKey;

        /// <summary>
        /// If used at the class level, a non-null value to use as the shared 
        /// partition key for all entities.
        /// </summary>
        public string? PartitionKey { get; }

        /// <summary>
        /// Creates a strong-typed fast accessor for the property annotated 
        /// with <see cref="PartitionKeyAttribute"/> for instances of the given type 
        /// <typeparamref name="TEntity"/>.
        /// </summary>
        public static Func<TEntity, string> CreateAccessor<TEntity>() => CreateAccessor<TEntity, PartitionKeyAttribute>();
    }
}
