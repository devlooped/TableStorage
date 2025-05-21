using System;

namespace Devlooped;

/// <summary>
/// Opts-in to receiving the document timestamp from the <see cref="IDocumentRepository{T}"/> 
/// when retrieving the document from the table storage.
/// </summary>
public interface IDocumentTimestamp
{
    /// <summary>
    /// The timestamp of the document.
    /// </summary>
    DateTimeOffset? Timestamp { get; set; }
}
