using FFCEI.DotNetStack.Serialization;
using System.Collections.ObjectModel;

namespace FFCEI.DotNetStack.MessageBroker.Configurations;

public sealed class MessageBrokerAutomaticReceiverConfiguration
{
    public required MessageBrokerAddress Address { get; set; }
    public ReadOnlyCollection<MessageBrokerAutomaticReceiverHandlerConfiguration>? Handlers { get; set; }
    public string? Subject { get; set; }
    public SerializationAlgorithm? SerializationAlgorithm { get; set; }
}
