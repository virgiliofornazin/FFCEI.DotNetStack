using System.Collections.ObjectModel;

namespace FFCEI.DotNetStack.MessageBroker.Configurations;

public sealed class MessageBrokerConnectionQueueDeclarationConfiguration
{
    public required string QueueName { get; set; }
    public bool Durable { get; set; } = true;
    public bool Exclusive { get; set; }
    public bool AutoDelete { get; set; }
    public ReadOnlyDictionary<string, string>? Arguments { get; set; }
    public ReadOnlyCollection<MessageBrokerConnectionQueueBindingConfiguration>? Bindings { get; set; }
}
