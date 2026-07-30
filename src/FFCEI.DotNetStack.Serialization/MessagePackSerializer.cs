using FFCEI.DotNetStack.Serialization.Common;
using FFCEI.DotNetStack.Serialization.Defaults;
using MessagePack;
using System.Collections;

namespace FFCEI.DotNetStack.Serialization;

public sealed class MessagePackSerializer(MessagePackSerializerOptions? options)
    : Serializer
{
    public static readonly MessagePackSerializer Instance = new();
    public static readonly MessagePackSerializer TrustedUncompressedSerializer = new(MessagePackDefaults.TrustedUncompressedSerializationOptions);
    public static readonly MessagePackSerializer TrustedCompressedSerializer = new(MessagePackDefaults.TrustedCompressedSerializationOptions);
    public static readonly MessagePackSerializer UntrustedUncompressedSerializer = new(MessagePackDefaults.UntrustedUncompressedSerializationOptions);
    public static readonly MessagePackSerializer UntrustedCompressedSerializer = new(MessagePackDefaults.UntrustedCompressedSerializationOptions);

    private readonly MessagePackSerializerOptions? _options = options;

    public MessagePackSerializer()
        : this(null)
    {
    }

    public override SerializationAlgorithm SerializationAlgorithm => SerializationAlgorithm.MessagePack;

    public override void SerializeAs(Type type, object? value, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        MessagePack.MessagePackSerializer.Serialize(type, stream, value,
            options: _options ?? MessagePackDefaults.UntrustedCompressedSerializationOptions);
    }

    public override void SerializeAs(Type type, object? value, ref byte[] buffer)
        => buffer = MessagePack.MessagePackSerializer.Serialize(type, value,
            options: _options ?? MessagePackDefaults.UntrustedCompressedSerializationOptions);

    public override void SerializeCollectionAs(Type type, IEnumerable? value, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        MessagePack.MessagePackSerializer.Serialize(type, stream, value,
            options: _options ?? MessagePackDefaults.UntrustedCompressedSerializationOptions);
    }

    public override void SerializeCollectionAs(Type type, IEnumerable? value, ref byte[] buffer)
        => buffer = MessagePack.MessagePackSerializer.Serialize(type, value,
            options: _options ?? MessagePackDefaults.UntrustedCompressedSerializationOptions);
}
