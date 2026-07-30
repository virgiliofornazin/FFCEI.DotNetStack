using FFCEI.DotNetStack.Serialization.Common;
using FFCEI.DotNetStack.Serialization.Defaults;
using System.Buffers;
using System.Text.Json;

namespace FFCEI.DotNetStack.Serialization;

public sealed class JsonDeserializer(JsonSerializerOptions? options)
    : StringPayloadDeserializer
{
    public static readonly JsonDeserializer Default = new();
    public static readonly JsonDeserializer ForDeserialization = new(JsonDefaults.ForDeserialization);

    private readonly JsonSerializerOptions? _options = options;

    public JsonDeserializer()
        : this(null)
    {
    }

    public override SerializationAlgorithm SerializationAlgorithm => SerializationAlgorithm.Json;

    public override object? DeserializeAs(Type type, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        return System.Text.Json.JsonSerializer.Deserialize(stream, type,
            options: _options ?? JsonDefaults.ForDeserialization);
    }

    public override object? DeserializeAs(Type type, ReadOnlyMemory<byte> buffer)
        => System.Text.Json.JsonSerializer.Deserialize(buffer.Span, type,
            options: _options ?? JsonDefaults.ForDeserialization);

    public override object? DeserializeAs(Type type, ReadOnlySpan<byte> buffer)
        => System.Text.Json.JsonSerializer.Deserialize(buffer, type,
            options: _options ?? JsonDefaults.ForDeserialization);

    public override object? DeserializeAs(Type type, ReadOnlySequence<byte> buffer)
        => System.Text.Json.JsonSerializer.Deserialize(buffer.ToArray(), type,
            options: _options ?? JsonDefaults.ForDeserialization);

    public override object? DeserializeAs(Type type, byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));

        return System.Text.Json.JsonSerializer.Deserialize(buffer, type,
            options: _options ?? JsonDefaults.ForDeserialization);
    }

    public override object? DeserializeAs(Type type, string buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));

        return System.Text.Json.JsonSerializer.Deserialize(buffer, type,
            options: _options ?? JsonDefaults.ForDeserialization);
    }
}
