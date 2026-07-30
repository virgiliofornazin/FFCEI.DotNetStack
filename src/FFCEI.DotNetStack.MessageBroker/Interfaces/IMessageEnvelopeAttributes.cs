namespace FFCEI.DotNetStack.MessageBroker.Interfaces;

public interface IMessageEnvelopeAttributes
    : IMessageAttributes
{
    string? TraceParent { get; }
    string? TraceState { get; }
    string? Baggage { get; }
    Guid MessageId { get; }
    DateTimeOffset? SentTimestamp { get; }
    DateTimeOffset? ReceivedTimestamp { get; }
    string? PayloadType { get; }

    bool Expired();
}
