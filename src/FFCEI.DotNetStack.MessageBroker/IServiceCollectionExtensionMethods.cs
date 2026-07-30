using FFCEI.DotNetStack.MessageBroker.Common;
using FFCEI.DotNetStack.MessageBroker.Configurations;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FFCEI.DotNetStack.MessageBroker;

public static class IServiceCollectionExtensionMethods
{
    public static IServiceCollection AddMessageBrokerConnectorFactory(this IServiceCollection services,
        MessageBrokerConnectorFactoryConfiguration configuration,
        Action<IMessageHandlerCallback>? callbackBuilder = null,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        logger ??= NullLogger.Instance;

        _ = services.AddSingleton<IMessageBrokerConnectorFactory>(serviceProvider =>
        {
            var messageBrokerConnectorFactory = new MessageBrokerConnectorFactory(configuration)
            {
                LoggerFactory = serviceProvider.GetService<ILoggerFactory>()
            };

            if ((messageBrokerConnectorFactory.LoggerFactory != null) &&
                (messageBrokerConnectorFactory.Logger == NullLogger.Instance))
            {
                messageBrokerConnectorFactory.Logger = messageBrokerConnectorFactory.LoggerFactory.CreateLogger(messageBrokerConnectorFactory.GetType());
            }

            callbackBuilder?.Invoke(messageBrokerConnectorFactory);

            return messageBrokerConnectorFactory;
        });

        MessageBrokerLog.MessageBrokerConnectionFactoryConfigured(logger);

        return services;
    }
}
