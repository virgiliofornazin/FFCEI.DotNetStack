using FFCEI.DotNetStack.Serialization.Common;
using FFCEI.DotNetStack.Serialization.Defaults;
using MessagePack;
using System.Buffers;

namespace FFCEI.DotNetStack.Serialization;

public sealed class MessagePackDeserializer(MessagePackSerializerOptions? options)
    : Deserializer
{
    public static readonly MessagePackDeserializer Instance = new();
    public static readonly MessagePackDeserializer TrustedUncompressedDeserializer = new(MessagePackDefaults.TrustedUncompressedSerializationOptions);
    public static readonly MessagePackDeserializer TrustedCompressedDeserializer = new(MessagePackDefaults.TrustedCompressedSerializationOptions);
    public static readonly MessagePackDeserializer UntrustedUncompressedDeserializer = new(MessagePackDefaults.UntrustedUncompressedSerializationOptions);
    public static readonly MessagePackDeserializer UntrustedCompressedDeserializer = new(MessagePackDefaults.UntrustedCompressedSerializationOptions);

    private readonly MessagePackSerializerOptions? _options = options;

    public MessagePackDeserializer()
        : this(null)
    {
    }

    public override SerializationAlgorithm SerializationAlgorithm => SerializationAlgorithm.MessagePack;

    public override object? DeserializeAs(Type type, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        return MessagePack.MessagePackSerializer.Deserialize(type, stream,
            options: _options ?? MessagePackDefaults.UntrustedCompressedSerializationOptions);
    }

    public override object? DeserializeAs(Type type, ReadOnlyMemory<byte> buffer)
        => MessagePack.MessagePackSerializer.Deserialize(type, buffer,
            options: _options ?? MessagePackDefaults.UntrustedCompressedSerializationOptions);

    public override object? DeserializeAs(Type type, ReadOnlySpan<byte> buffer)
        => MessagePack.MessagePackSerializer.Deserialize(type, buffer.ToArray(),
            options: _options ?? MessagePackDefaults.UntrustedCompressedSerializationOptions);

    public override object? DeserializeAs(Type type, ReadOnlySequence<byte> buffer)
        => MessagePack.MessagePackSerializer.Deserialize(type, buffer,
            options: _options ?? MessagePackDefaults.UntrustedCompressedSerializationOptions);

    public override object? DeserializeAs(Type type, byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));

        return MessagePack.MessagePackSerializer.Deserialize(type, buffer,
            options: _options ?? MessagePackDefaults.UntrustedCompressedSerializationOptions);
    }
}
