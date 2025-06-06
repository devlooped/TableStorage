﻿//<auto-generated/>
#nullable enable
using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace Devlooped
{
    /// <summary>
    /// Default implementation of <see cref="IBinaryDocumentSerializer"/> which 
    /// uses Newtonsoft.Json implementation of BSON for serialization.
    /// </summary>
    partial class MessagePackDocumentSerializer : IBinaryDocumentSerializer
    {
        /// <summary>
        /// Default instance of the serializer using default serialization options.
        /// </summary>
        public static IDocumentSerializer Default { get; } = new MessagePackDocumentSerializer();

        readonly MessagePackSerializerOptions? options;

        /// <summary>
        /// Initializes the document serializer with the given optional serializer options.
        /// </summary>
        public MessagePackDocumentSerializer(MessagePackSerializerOptions? options = default)
        {
            this.options = options;
#if NET6_0_OR_GREATER
            this.options = (options ?? MessagePackSerializerOptions.Standard)
                .WithResolver(CompositeResolver.Create(
                    StandardResolver.Instance,
                    DateOnlyFormatterResolver.Instance));
#endif
        }

        /// <inheritdoc />
        public T? Deserialize<T>(byte[] data) => data.Length == 0 ? default : MessagePackSerializer.Deserialize<T>(data, options);

        /// <inheritdoc />
        public byte[] Serialize<T>(T value) => value == null ? new byte[0] : MessagePackSerializer.Serialize(value.GetType(), value, options);

#if NET6_0_OR_GREATER
        internal class DateOnlyFormatterResolver : IFormatterResolver
        {
            public static IFormatterResolver Instance = new DateOnlyFormatterResolver();

            public IMessagePackFormatter<T> GetFormatter<T>()
            {
                if (typeof(T) == typeof(DateOnly))
                    return (IMessagePackFormatter<T>)DateOnlyFormatter.Instance;

                return null!;
            }

            internal class DateOnlyFormatter : IMessagePackFormatter<DateOnly>
            {
                public static readonly IMessagePackFormatter<DateOnly> Instance = new DateOnlyFormatter();

                public void Serialize(ref MessagePackWriter writer, DateOnly value, MessagePackSerializerOptions options)
                    => writer.Write(value.DayNumber);

                public DateOnly Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
                    => DateOnly.FromDayNumber(reader.ReadInt32());
            }
        }
#endif
    }
}
