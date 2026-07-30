using System.Collections.ObjectModel;

namespace FFCEI.DotNetStack.MessageBroker.Configurations;

public sealed class MessageBrokerConnectionExchangeDeclarationConfiguration
{
    public required string ExchangeName { get; set; }
    public required MessageBrokerAddressType ExchangeType { get; set; }
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; }
    public ReadOnlyDictionary<string, string>? Arguments { get; set; }
}
