using System;
using System.Runtime.Serialization;

namespace ProtobufNetTest
{
    [DataContract(Name = nameof(DateTimeOffset))]
    public class DateTimeOffsetSurrogate
    {
        [DataMember(Order = 1)]
        public long? Value { get; set; }

        public static implicit operator DateTimeOffset(DateTimeOffsetSurrogate surrogate)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(surrogate.Value.GetValueOrDefault());
        }

        public static implicit operator DateTimeOffset?(DateTimeOffsetSurrogate surrogate)
        {
            return surrogate != null ? DateTimeOffset.FromUnixTimeMilliseconds(surrogate.Value.GetValueOrDefault()) : null;
        }

        public static implicit operator DateTimeOffsetSurrogate(DateTimeOffset source)
        {
            return new DateTimeOffsetSurrogate
            {
                Value = source.ToUnixTimeMilliseconds()
            };
        }

        public static implicit operator DateTimeOffsetSurrogate(DateTimeOffset? source)
        {
            return new DateTimeOffsetSurrogate
            {
                Value = source?.ToUnixTimeMilliseconds()
            };
        }
    }
}