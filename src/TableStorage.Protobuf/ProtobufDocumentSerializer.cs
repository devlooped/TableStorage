﻿//<auto-generated/>
#nullable enable
using System;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Devlooped
{
    /// <summary>
    /// Default implementation of <see cref="IBinaryDocumentSerializer"/> which 
    /// uses Newtonsoft.Json implementation of BSON for serialization.
    /// </summary>
    partial class ProtobufDocumentSerializer : IBinaryDocumentSerializer
    {
        /// <summary>
        /// Default instance of the serializer.
        /// </summary>
        public static IDocumentSerializer Default { get; } = new ProtobufDocumentSerializer();

        /// <inheritdoc />
        public T? Deserialize<T>(byte[] data)
        {
            if (data.Length == 0)
                return default;

            using var mem = new MemoryStream(data);
            return (T?)Serializer.Deserialize(typeof(T), mem);
        }

        /// <inheritdoc />
        public byte[] Serialize<T>(T value)
        {
            if (value == null)
                return new byte[0];

            using var mem = new MemoryStream();
            Serializer.Serialize(mem, value);
            return mem.ToArray();
        }
    }
}
