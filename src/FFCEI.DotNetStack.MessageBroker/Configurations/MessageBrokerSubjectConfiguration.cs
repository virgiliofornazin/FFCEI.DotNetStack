using FFCEI.DotNetStack.Serialization;

namespace FFCEI.DotNetStack.MessageBroker.Configurations;

public sealed class MessageBrokerSubjectConfiguration
{
    public required string Name { get; set; }
    public string? ConnectionId { get; set; }
    public SerializationAlgorithm? SerializationAlgorithm { get; set; }
    public bool IsDefault { get; set; }
}
