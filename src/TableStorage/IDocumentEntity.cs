﻿//<auto-generated/>
#nullable enable
using System;
using Azure;
using Azure.Data.Tables;

namespace Devlooped
{
    /// <summary>
    /// Document metadata for <see cref="IDocumentRepository{T}"/> querying purposes.
    /// </summary>
    partial interface IDocumentEntity : ITableEntity
    {
        /// <summary>
        /// The type of the document, its <see cref="System.Type.FullName"/>.
        /// </summary>
        /// <remarks>
        /// For nested types, the '+' will be replaced with '.'.
        /// </remarks>
        string? Type { get; }
        
        /// <summary>
        /// The major.minor version of the assembly the document type belongs to.
        /// </summary>
        string? Version { get; }
        
        /// <summary>
        /// The major component of the <see cref="Version"/>.
        /// </summary>
        int? MajorVersion { get; }

        /// <summary>
        /// The minor component of the <see cref="Version"/>.
        /// </summary>
        int? MinorVersion { get; }
    }

    internal class BinaryDocumentEntity : ITableEntity, IDocumentEntity
    {
        public BinaryDocumentEntity() { }
        public BinaryDocumentEntity(string partitionKey, string rowKey)
            => (PartitionKey, RowKey)
            = (partitionKey, rowKey);

        public byte[]? Document { get; set; }
        public string? Type { get; set; }
        public string? Version { get; set; }
        public int? MajorVersion { get; set; }
        public int? MinorVersion { get; set; }

        public string PartitionKey { get; set; } = "";
        public string RowKey { get; set; } = "";
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; } = ETag.All;
    }

    internal class StringDocumentEntity : ITableEntity, IDocumentEntity
    {
        public StringDocumentEntity() { }
        public StringDocumentEntity(string partitionKey, string rowKey)
            => (PartitionKey, RowKey)
            = (partitionKey, rowKey);

        public string? Document { get; set; }
        public string? Type { get; set; }
        public string? Version { get; set; }
        public int? MajorVersion { get; set; }
        public int? MinorVersion { get; set; }

        public string PartitionKey { get; set; } = "";
        public string RowKey { get; set; } = "";
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; } = ETag.All;
    }

}
