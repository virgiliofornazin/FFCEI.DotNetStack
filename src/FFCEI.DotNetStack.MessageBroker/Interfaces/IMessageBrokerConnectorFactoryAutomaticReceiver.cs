using System.Reflection;

namespace FFCEI.DotNetStack.MessageBroker.Interfaces;

public interface IMessageBrokerConnectorFactoryAutomaticReceiver
{
    Task StartAutomaticReceiversAsync(IEnumerable<Assembly> assembliesToLoadHandlers, CancellationToken cancellationToken = default);
    Task StartAutomaticReceiversAsync(CancellationToken cancellationToken = default)
        => StartAutomaticReceiversAsync([], cancellationToken);
}
