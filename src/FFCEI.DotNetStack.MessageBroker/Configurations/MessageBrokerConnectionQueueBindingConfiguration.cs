using System.Collections.ObjectModel;

namespace FFCEI.DotNetStack.MessageBroker.Configurations;

public sealed class MessageBrokerConnectionQueueBindingConfiguration
{
    public required MessageBrokerAddress BindingAddress { get; set; }
    public ReadOnlyDictionary<string, string>? Arguments { get; set; }
}
