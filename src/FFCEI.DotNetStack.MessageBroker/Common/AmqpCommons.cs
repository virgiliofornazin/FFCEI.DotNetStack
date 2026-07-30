namespace FFCEI.DotNetStack.MessageBroker.Common;

internal static class AmqpCommons
{
    internal static void ExtractExchangeAndRoutingKey(MessageBrokerAddress address, out string exchange, out string routingKey)
        => (exchange, routingKey) = address.Type switch
        {
            MessageBrokerAddressType.CustomExchange => !string.IsNullOrWhiteSpace(address.CustomExchange) ?
                (address.CustomExchange, address.Value) : throw new NotSupportedException($"CustomExchange not defined"),
            MessageBrokerAddressType.Direct => ("amq.direct", address.Value),
            MessageBrokerAddressType.Topic => ("amq.topic", address.Value),
            MessageBrokerAddressType.Queue => (string.Empty, address.Value),
            MessageBrokerAddressType.Headers => ("amq.match", string.Empty),
            MessageBrokerAddressType.Fanout => ("amq.fanout", string.Empty),
            _ => throw new NotSupportedException($"Address type '{address.Type}' is not supported")
        };
}
