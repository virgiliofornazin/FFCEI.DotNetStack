using System.Diagnostics;

namespace FFCEI.DotNetStack.MessageBroker.Configurations;

[DebuggerDisplay("MessageBrokerAutomaticReceiverHandler: {HandlerTypename,nq} [{AssemblyFileName,nq}]")]
public sealed class MessageBrokerAutomaticReceiverHandlerConfiguration
{
    public required string HandlerTypename { get; set; }
    public string? AssemblyFileName { get; set; }
}
