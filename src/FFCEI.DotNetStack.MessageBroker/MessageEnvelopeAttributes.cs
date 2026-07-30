using FFCEI.DotNetStack.MessageBroker.Interfaces;
using System.Collections.ObjectModel;

namespace FFCEI.DotNetStack.MessageBroker;

public sealed class MessageEnvelopeAttributes
    : IMessageEnvelopeAttributes
{
    internal static readonly ReadOnlyDictionary<string, object> EmptyRoutingHeaders = new(new Dictionary<string, object>());

    internal MessageEnvelopeAttributes()
    {
    }

    internal void Assign(IMessageAttributes messageAttributes)
    {
        RoutingHeaders = messageAttributes.RoutingHeaders;
        TTL = messageAttributes.TTL;
        Durable = messageAttributes.Durable;
    }

    public string? TraceParent { get; internal set; }
    public string? TraceState { get; internal set; }
    public string? Baggage { get; internal set; }
    public IReadOnlyDictionary<string, object>? RoutingHeaders { get; internal set; }
    public TimeSpan? TTL { get; internal set; }
    public bool Durable { get; internal set; }
    public Guid MessageId { get; internal set; }
    public DateTimeOffset? SentTimestamp { get; internal set; }
    public DateTimeOffset? ReceivedTimestamp { get; internal set; }
    public string? PayloadType { get; internal set; }

    internal MessageEnvelopeAttributes Clone()
        => new()
        {
            TraceParent = TraceParent,
            TraceState = TraceState,
            Baggage = Baggage,
            RoutingHeaders = RoutingHeaders,
            TTL = TTL,
            Durable = Durable,
            MessageId = MessageId,
            SentTimestamp = SentTimestamp,
            ReceivedTimestamp = ReceivedTimestamp,
            PayloadType = PayloadType
        };

    internal DateTimeOffset TouchSentTimestamp()
    {
        SentTimestamp ??= DateTimeOffset.UtcNow;

        return SentTimestamp.Value;
    }

    internal DateTimeOffset TouchReceivedTimestamp()
    {
        ReceivedTimestamp ??= DateTimeOffset.UtcNow;

        return ReceivedTimestamp.Value;
    }

    internal MessageEnvelopeAttributes FixTimestamps()
    {
        SentTimestamp ??= TouchReceivedTimestamp();

        return this;
    }

    public bool Expired()
        => TTL.HasValue && SentTimestamp.HasValue && ((DateTimeOffset.UtcNow - SentTimestamp.Value) > TTL.Value);
}
