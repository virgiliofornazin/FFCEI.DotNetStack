using FFCEI.DotNetStack.Serialization.Common;
using FFCEI.DotNetStack.Serialization.Defaults;
using FFCEI.DotNetStack.Serialization.Interfaces;
using System.Collections;
using System.Text.Json;

namespace FFCEI.DotNetStack.Serialization;

public sealed class JsonSerializer(JsonSerializerOptions? options)
    : Serializer, IStringPayloadSerializer
{
    public static readonly JsonSerializer Default = new();
    public static readonly JsonSerializer OptimizedForMessagingInstance = new(JsonDefaults.OptimizedForMessaging);
    public static readonly JsonSerializer OptimizedForScreenInstance = new(JsonDefaults.OptimizedForScreen);
    public static readonly JsonSerializer FormattedForScreenInstance = new(JsonDefaults.FormattedForScreen);

    private readonly JsonSerializerOptions? _options = options;

    public JsonSerializer()
        : this(null)
    {
    }

    public override SerializationAlgorithm SerializationAlgorithm => SerializationAlgorithm.Json;

    public override void SerializeAs(Type type, object? value, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        System.Text.Json.JsonSerializer.Serialize(stream, value, type,
            options: _options ?? JsonDefaults.OptimizedForMessaging);
    }

    public override void SerializeAs(Type type, object? value, ref byte[] buffer)
        => buffer = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value, type,
            options: _options ?? JsonDefaults.OptimizedForMessaging);

    public override void SerializeCollectionAs(Type type, IEnumerable? value, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        System.Text.Json.JsonSerializer.Serialize(stream, value, type,
            options: _options ?? JsonDefaults.OptimizedForMessaging);
    }

    public override void SerializeCollectionAs(Type type, IEnumerable? value, ref byte[] buffer)
        => buffer = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value, type,
            options: _options ?? JsonDefaults.OptimizedForMessaging);
}
