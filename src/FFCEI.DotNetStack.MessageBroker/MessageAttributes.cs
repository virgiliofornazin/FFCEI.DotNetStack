using FFCEI.DotNetStack.MessageBroker.Interfaces;

namespace FFCEI.DotNetStack.MessageBroker;

public sealed class MessageAttributes
    : IMessageAttributes
{
    public IReadOnlyDictionary<string, object>? RoutingHeaders { get; set; }
    public TimeSpan? TTL { get; set; }
    public bool Durable { get; set; }
}
