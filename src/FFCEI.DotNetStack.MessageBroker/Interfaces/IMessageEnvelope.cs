namespace FFCEI.DotNetStack.MessageBroker.Interfaces;

public interface IMessageEnvelope
{
    IMessageEnvelopeAttributes Attributes { get; }
    object? Payload { get; }
    ReadOnlyMemory<byte>? EncodedPayload { get; }

    bool Expired();
}
