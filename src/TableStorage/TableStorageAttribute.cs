﻿//<auto-generated/>
#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Devlooped
{
    abstract partial class TableStorageAttribute : Attribute
    {
        static readonly ConcurrentDictionary<(Type EntityType, Type AttributeType), Expression> getters = new();
        static readonly ConcurrentDictionary<(Type EntityType, Type AttributeType), Delegate> compiledGetters = new();

        // See https://stackoverflow.com/questions/11514707/azure-table-storage-rowkey-restricted-character-patterns
        static readonly HashSet<char> InvalidChars = new HashSet<char>(new[]
        {
            ' ', '/', '\\', '#', '?', '\t', '\n', '\r', '+', '|', '[', ']', '{', '}', '<', '>', '$', '^', '&'
        });

        protected static Expression<Func<TEntity, string>> CreateGetter<TEntity, TAttribute>() where TAttribute : Attribute
            => (Expression<Func<TEntity, string>>)getters.GetOrAdd((typeof(TEntity), typeof(TAttribute)), _ => CreateGetterCore<TEntity, TAttribute>());

        protected static Func<TEntity, string> CreateCompiledGetter<TEntity, TAttribute>() where TAttribute : Attribute
            => (Func<TEntity, string>)compiledGetters.GetOrAdd((typeof(TEntity), typeof(TAttribute)), _ => CreateGetter<TEntity, TAttribute>().Compile());

        static Expression<Func<TEntity, string>> CreateGetterCore<TEntity, TAttribute>() where TAttribute : Attribute
        {
            var attributeName = typeof(TAttribute).Name.Substring(0, typeof(TAttribute).Name.Length - 9);

            var keyProp = typeof(TEntity).GetProperties()
                .FirstOrDefault(prop => prop.GetCustomAttribute<TAttribute>() != null);

            // See if the attribute is applied to a constructor argument with the same name 
            // as a prop, like it would in a record type
            if (keyProp == null)
            {
                var keyParam = typeof(TEntity).GetConstructors().SelectMany(ctor => ctor.GetParameters())
                    .Where(prm => prm.GetCustomAttribute<TAttribute>() != null)
                    .FirstOrDefault();

                if (keyParam != null)
                    keyProp = typeof(TEntity).GetProperties().FirstOrDefault(prop => prop.Name == keyParam.Name);
            }

            if (keyProp == null)
            {
                if (typeof(TAttribute) == typeof(PartitionKeyAttribute))
                {
                    // See if the attribute is applied at the class level with a fixed value
                    var partitionKey = typeof(TEntity).GetCustomAttribute<PartitionKeyAttribute>()?.PartitionKey;
                    if (partitionKey == null)
                    {
                        partitionKey = typeof(TEntity).Name;
                        if (partitionKey.EndsWith("Entity", StringComparison.OrdinalIgnoreCase))
                            partitionKey = partitionKey.Substring(0, partitionKey.Length - 6);
                    }

                    return _ => partitionKey;
                }

                throw new ArgumentException($"Expected entity type '{typeof(TEntity).Name}' to have one property annotated with [{attributeName}]");
            }

            if (keyProp.PropertyType != typeof(string))
                throw new ArgumentException($"Property '{typeof(TEntity).Name}.{keyProp.Name}' annotated with [{attributeName}] must be of type string.");

            var param = Expression.Parameter(typeof(TEntity), "entity");

            return Expression.Lambda<Func<TEntity, string>>(
                Expression.Block(
                    Expression.IfThen(
                        Expression.Equal(param, Expression.Constant(null)),
                        Expression.Throw(
                            Expression.New(
                                typeof(ArgumentNullException).GetConstructor(new[] { typeof(string) })!,
                                Expression.Constant("entity")))),
                    Expression.Call(
                        typeof(TableStorageAttribute).GetMethod(nameof(EnsureValid), BindingFlags.NonPublic | BindingFlags.Static)!,
                        Expression.Constant(attributeName),
                        Expression.Constant(keyProp.Name, typeof(string)),
                        Expression.Property(param, keyProp))),
                param);
        }

        static string EnsureValid(string attributeName, string propertyName, string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), $"[{attributeName}]-annotated property '{propertyName}' cannot be null.");

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"[{attributeName}]-annotated property '{propertyName}' cannot be empty.", nameof(value));

            if (value.Any(c => char.IsControl(c) || InvalidChars.Contains(c)))
                throw new ArgumentException($"Property '{propertyName}' has value '{value}', which contains invalid characters for [{attributeName}].", nameof(value));

            return value;
        }

        /// <summary>
        /// Sanitizes the value so that it can be used as a key in table storage (either PartitionKey or RowKey).
        /// </summary>
        public static string Sanitize(string value) => new string(value.Where(c => !char.IsControl(c) && !InvalidChars.Contains(c)).ToArray());
    }
}
