namespace FFCEI.DotNetStack.MessageBroker.Interfaces;

public interface IMessageAttributes
{
    IReadOnlyDictionary<string, object>? RoutingHeaders { get; }
    TimeSpan? TTL { get; }
    bool Durable { get; }
}
