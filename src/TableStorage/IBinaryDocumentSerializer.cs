﻿//<auto-generated/>
#nullable enable

namespace Devlooped
{
    /// <summary>
    /// Serializes an object to/from a byte array.
    /// </summary>
    partial interface IBinaryDocumentSerializer : IDocumentSerializer
    {
        /// <summary>
        /// Serializes the value to a byte array.
        /// </summary>
        byte[] Serialize<T>(T value);

        /// <summary>
        /// Deserializes an object from its serialized data.
        /// </summary>
        T? Deserialize<T>(byte[] data);
    }
}
