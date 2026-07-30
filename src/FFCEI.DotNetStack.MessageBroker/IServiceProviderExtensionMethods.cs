using FFCEI.DotNetStack.MessageBroker.Common;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FFCEI.DotNetStack.MessageBroker;

public static class IServiceProviderExtensionMethods
{
    public static Task UseMessageBrokerConnectorsAsync(this IServiceProvider serviceProvider, IEnumerable<Assembly> assembliesToLoadHandlers, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assembliesToLoadHandlers, nameof(assembliesToLoadHandlers));

        var factory = serviceProvider.GetRequiredService<IMessageBrokerConnectorFactory>();

        if (factory is not MessageBrokerConnectorFactory messageBrokerConnectorFactory)
        {
            throw new InvalidOperationException($"Unknown IMessageBrokerConnectorFactory type {factory.GetType()}");
        }

        return messageBrokerConnectorFactory.StartAutomaticReceiversAsync(assembliesToLoadHandlers, cancellationToken);
    }

    public static Task UseMessageBrokerConnectorsAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var factory = serviceProvider.GetRequiredService<IMessageBrokerConnectorFactory>();

        if (factory is not MessageBrokerConnectorFactory messageBrokerConnectorFactory)
        {
            throw new InvalidOperationException($"Unknown IMessageBrokerConnectorFactory type {factory.GetType()}");
        }

        return messageBrokerConnectorFactory.StartAutomaticReceiversAsync(cancellationToken);
    }
}
