﻿using System;

namespace Devlooped
{
    /// <summary>
    /// Flags the property to use as the table storage row key 
    /// when storing the annotated type using the <see cref="TableRepository{T}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    partial class RowKeyAttribute : TableStorageAttribute
    {
        /// <summary>
        /// Creates a strong-typed fast accessor for the property annotated 
        /// with <see cref="RowKeyAttribute"/> for instances of the given type 
        /// <typeparamref name="TEntity"/>.
        /// </summary>
        public static Func<TEntity, string> CreateAccessor<TEntity>() => CreateAccessor<TEntity, RowKeyAttribute>();
    }
}
