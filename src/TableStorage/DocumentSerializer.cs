﻿//<auto-generated/>
#nullable enable
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Devlooped
{
    /// <summary>
    /// Default implementation of <see cref="IStringDocumentSerializer"/> which 
    /// uses System.Text.Json for serialization.
    /// </summary>
    partial class DocumentSerializer : IStringDocumentSerializer
    {
        /// <summary>
        /// Default serializer options to use for the <see cref="Default"/> singleton document serialier.
        /// </summary>
        /// <remarks>
        /// Default settings are: <see cref="JsonSerializerOptions.AllowTrailingCommas"/> = true, 
        /// <see cref="JsonSerializerOptions.DefaultIgnoreCondition"/> = <see cref="JsonIgnoreCondition.WhenWritingDefault"/> and 
        /// <see cref="JsonSerializerOptions.WriteIndented"/> = <see langword="true"/>
        /// </remarks>
        public static JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = true,
            Converters =
            {
                // Enums should persist/parse with their string values instead
                new JsonStringEnumConverter(allowIntegerValues: false),
#if NET6_0_OR_GREATER
                new DateOnlyJsonConverter()
#endif
            }
        };

#if NET6_0_OR_GREATER
        public class DateOnlyJsonConverter : JsonConverter<DateOnly>
        {
            public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) 
                => DateOnly.Parse(reader.GetString()?.Substring(0, 10) ?? "", CultureInfo.InvariantCulture);
            
            public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) 
                => writer.WriteStringValue(value.ToString("O", CultureInfo.InvariantCulture));
        }
#endif

        /// <summary>
        /// Default instance of the serializer.
        /// </summary>
        public static IStringDocumentSerializer Default { get; } = new DocumentSerializer();

        readonly JsonSerializerOptions options;

        /// <summary>
        /// Initializes the document serializer using the <see cref="DefaultOptions"/>.
        /// </summary>
        public DocumentSerializer()
            : this(DefaultOptions) { }

        /// <summary>
        /// Initializes the document serializer using the given <see cref="JsonSerializerOptions"/>.
        /// </summary>
        public DocumentSerializer(JsonSerializerOptions options) => this.options = options;

        /// <inheritdoc />
        public T? Deserialize<T>(string data) => JsonSerializer.Deserialize<T>(data, options);

        /// <inheritdoc />
        public string Serialize<T>(T value) => JsonSerializer.Serialize(value, options);
    }
}
