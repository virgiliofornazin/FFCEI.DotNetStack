using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;
using FFCEI.DotNetStack.MessageBroker.Configurations;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using FFCEI.DotNetStack.Serialization;
using FFCEI.DotNetStack.Serialization.Interfaces;

namespace FFCEI.DotNetStack.MessageBroker;

public sealed class MessageEnvelope
    : IMessageEnvelope
{
    public MessageEnvelopeAttributes Attributes { get; internal set; } = new();
    internal byte[]? PayloadBytes { get; set; }
    internal Type? PayloadType { get => field ?? Payload?.GetType(); set; }

    internal MessageEnvelope()
    {
    }

    IMessageEnvelopeAttributes IMessageEnvelope.Attributes => Attributes;
    public object? Payload { get; set; }
    public ReadOnlyMemory<byte>? EncodedPayload
    {
        get => PayloadBytes ?? field;
        set;
    }

    public bool Expired()
        => Attributes.Expired();

    internal MessageEnvelope Clone()
        => new()
        {
            Attributes = Attributes.Clone(),
            PayloadBytes = PayloadBytes,
            EncodedPayload = EncodedPayload,
            Payload = Payload,
            PayloadType = PayloadType
        };

    internal static MessageEnvelope CreateForSend(object payload, IMessageAttributes? messageAttributes)
    {
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        var result = new MessageEnvelope()
        {
            Payload = payload
        };

        var payloadType = payload.GetType();

        if (messageAttributes != null)
        {
            result.Attributes.Assign(messageAttributes);
        }

        result.Attributes.MessageId = Guid.CreateVersion7();
        result.Attributes.PayloadType = payloadType.NamespaceAndName();

        MessageBrokerTracing.CaptureCurrentActivity(result.Attributes);

        return result;
    }

    public void SerializeEnvelope(ISerializer? serializer)
    {
        _ = Attributes.TouchSentTimestamp();

        ArgumentNullException.ThrowIfNull(Payload, nameof(Payload));

        serializer ??= SerializationConfiguration.DefaultSerializer;

        PayloadBytes = serializer.SerializeToBytes(Payload);
    }

    public bool DeserializeEnvelope(IDeserializer? deserializer, MessageHandlerRegistry? messageHandlerRegistry = null)
    {
        _ = Attributes.TouchReceivedTimestamp();

        ArgumentNullException.ThrowIfNull(Attributes.PayloadType, nameof(Attributes.PayloadType));

        PayloadType = TypeExtensionMethods.FindType(Attributes.PayloadType) ??
            throw new InvalidOperationException($"Unknown payload type {Attributes.PayloadType}");

        var canHandleMessage = (messageHandlerRegistry?.CanHandleMessageOFType(PayloadType) ?? false) ||
            MessageHandlerRegistry.Global.CanHandleMessageOFType(PayloadType);

        if (!canHandleMessage)
        {
            Payload = null;

            return false;
        }

        if (EncodedPayload == null)
        {
            throw new InvalidOperationException($"EncodedPayload not defined");
        }

        deserializer ??= SerializationConfiguration.DefaultDeserializer;

        Payload = deserializer.DeserializeAs(PayloadType, EncodedPayload.Value);

        return true;
    }
}
