using FFCEI.DotNetStack.Serialization;
using System.Collections.ObjectModel;

namespace FFCEI.DotNetStack.MessageBroker.Configurations;

public sealed class MessageBrokerConnectionConfiguration
{
    public string? ConnectionId { get; set; }
    public required MessageBrokerProviderType ProviderType { get; set; }
    public SerializationAlgorithm? SerializationAlgorithm { get; set; }
    public required string ConnectionString { get; set; }
    public bool IsDefault { get; set; }
    public ReadOnlyCollection<MessageBrokerConnectionExchangeDeclarationConfiguration>? ExchangeDeclarations { get; set; }
    public ReadOnlyCollection<MessageBrokerConnectionQueueDeclarationConfiguration>? QueueDeclarations { get; set; }
}
