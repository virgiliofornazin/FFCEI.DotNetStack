using FFCEI.DotNetStack.Serialization;
using FFCEI.DotNetStack.Serialization.Interfaces;

namespace FFCEI.DotNetStack.MessageBroker.Configurations;

internal static class SerializationConfiguration
{
    public static readonly ISerializer DefaultSerializer = MessagePackSerializer.UntrustedCompressedSerializer;
    public static readonly IDeserializer DefaultDeserializer = MessagePackDeserializer.UntrustedCompressedDeserializer;

    public static ISerializer? SerializerFrom(SerializationAlgorithm? serializationAlgorithm)
        => serializationAlgorithm switch
        {
            SerializationAlgorithm.Json => JsonSerializer.OptimizedForMessagingInstance,
            SerializationAlgorithm.MessagePack => MessagePackSerializer.TrustedCompressedSerializer,
            _ => null
        };

    public static IDeserializer? DeserializerFrom(SerializationAlgorithm? serializationAlgorithm)
        => serializationAlgorithm switch
        {
            SerializationAlgorithm.Json => JsonDeserializer.ForDeserialization,
            SerializationAlgorithm.MessagePack => MessagePackDeserializer.UntrustedCompressedDeserializer,
            _ => null
        };
}
