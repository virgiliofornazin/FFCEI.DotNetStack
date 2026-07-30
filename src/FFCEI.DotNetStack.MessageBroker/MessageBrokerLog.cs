using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.MessageBroker;

internal static partial class MessageBrokerLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "MessageBrokerConnectionFactory configured")]
    internal static partial void MessageBrokerConnectionFactoryConfigured(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Connecting MessageBroker connection '{ConnectionId}' using provider {ProviderType}...")]
    internal static partial void Connecting(ILogger logger, string connectionId, MessageBrokerProviderType providerType);

    [LoggerMessage(Level = LogLevel.Information, Message = "MessageBroker connection '{ConnectionId}' established (provider {ProviderType})")]
    internal static partial void Connected(ILogger logger, string connectionId, MessageBrokerProviderType providerType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to establish MessageBroker connection '{ConnectionId}' (provider {ProviderType})")]
    internal static partial void ConnectFailed(ILogger logger, Exception exception, string connectionId, MessageBrokerProviderType providerType);

    [LoggerMessage(Level = LogLevel.Information, Message = "[{ProviderType}] Connector started")]
    internal static partial void ConnectorStarted(ILogger logger, MessageBrokerProviderType providerType);

    [LoggerMessage(Level = LogLevel.Error, Message = "[{ProviderType}] Connector failed to start")]
    internal static partial void ConnectorStartFailed(ILogger logger, Exception exception, MessageBrokerProviderType providerType);

    [LoggerMessage(Level = LogLevel.Information, Message = "[{ProviderType}] Declared {Kind} '{Name}'")]
    internal static partial void Declared(ILogger logger, MessageBrokerProviderType providerType, string kind, string name);

    [LoggerMessage(Level = LogLevel.Error, Message = "[{ProviderType}] Failed to declare {Kind} '{Name}'")]
    internal static partial void DeclareFailed(ILogger logger, Exception exception, MessageBrokerProviderType providerType, string kind, string name);

    [LoggerMessage(Level = LogLevel.Error, Message = "[{ProviderType}] Failed to seek partition '{TopicPartition}' back to offset {Offset} for retry - retry counter for this partition may undercount")]
    internal static partial void SeekFailed(ILogger logger, Exception exception, MessageBrokerProviderType providerType, string topicPartition, long offset);

    [LoggerMessage(Level = LogLevel.Information, Message = "[{ProviderType}] Deleted {Kind} '{Name}'")]
    internal static partial void Deleted(ILogger logger, MessageBrokerProviderType providerType, string kind, string name);

    [LoggerMessage(Level = LogLevel.Error, Message = "[{ProviderType}] Failed to delete {Kind} '{Name}'")]
    internal static partial void DeleteFailed(ILogger logger, Exception exception, MessageBrokerProviderType providerType, string kind, string name);

    [LoggerMessage(Level = LogLevel.Information, Message = "[{ProviderType}] Declared binding of queue '{Queue}' to exchange '{Exchange}'")]
    internal static partial void DeclaredBinding(ILogger logger, MessageBrokerProviderType providerType, string queue, string exchange);

    [LoggerMessage(Level = LogLevel.Error, Message = "[{ProviderType}] Failed to declare binding of queue '{Queue}' to exchange '{Exchange}'")]
    internal static partial void DeclareBindingFailed(ILogger logger, Exception exception, MessageBrokerProviderType providerType, string queue, string exchange);

    [LoggerMessage(Level = LogLevel.Information, Message = "[{ProviderType}] Deleted binding of queue '{Queue}' to exchange '{Exchange}'")]
    internal static partial void DeletedBinding(ILogger logger, MessageBrokerProviderType providerType, string queue, string exchange);

    [LoggerMessage(Level = LogLevel.Error, Message = "[{ProviderType}] Failed to delete binding of queue '{Queue}' to exchange '{Exchange}'")]
    internal static partial void DeleteBindingFailed(ILogger logger, Exception exception, MessageBrokerProviderType providerType, string queue, string exchange);

    [LoggerMessage(Level = LogLevel.Information, Message = "[{ProviderType}] Declared subscription '{Subscription}' on topic '{Topic}'")]
    internal static partial void DeclaredSubscription(ILogger logger, MessageBrokerProviderType providerType, string subscription, string topic);

    [LoggerMessage(Level = LogLevel.Error, Message = "[{ProviderType}] Failed to declare subscription '{Subscription}' on topic '{Topic}'")]
    internal static partial void DeclareSubscriptionFailed(ILogger logger, Exception exception, MessageBrokerProviderType providerType, string subscription, string topic);

    [LoggerMessage(Level = LogLevel.Information, Message = "[{ProviderType}] Deleted subscription '{Subscription}' on topic '{Topic}'")]
    internal static partial void DeletedSubscription(ILogger logger, MessageBrokerProviderType providerType, string subscription, string topic);

    [LoggerMessage(Level = LogLevel.Error, Message = "[{ProviderType}] Failed to delete subscription '{Subscription}' on topic '{Topic}'")]
    internal static partial void DeleteSubscriptionFailed(ILogger logger, Exception exception, MessageBrokerProviderType providerType, string subscription, string topic);

    [LoggerMessage(Level = LogLevel.Information, Message = "[{ProviderType}] Sender created for subject '{Subject}'")]
    internal static partial void SenderCreated(ILogger logger, MessageBrokerProviderType providerType, string subject);

    [LoggerMessage(Level = LogLevel.Information, Message = "[{ProviderType}] Receiver created for subject '{Subject}'")]
    internal static partial void ReceiverCreated(ILogger logger, MessageBrokerProviderType providerType, string subject);
}
