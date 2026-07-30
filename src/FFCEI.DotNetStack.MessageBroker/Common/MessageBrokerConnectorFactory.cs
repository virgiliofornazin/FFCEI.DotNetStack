using Confluent.Kafka.Admin;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.AmazonSQS;
using FFCEI.DotNetStack.MessageBroker.AmazonSQSwithSNS;
using FFCEI.DotNetStack.MessageBroker.AzureServiceBus;
using FFCEI.DotNetStack.MessageBroker.Channels;
using FFCEI.DotNetStack.MessageBroker.Configurations;
using FFCEI.DotNetStack.MessageBroker.GooglePubSub;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using FFCEI.DotNetStack.MessageBroker.Kafka;
using FFCEI.DotNetStack.MessageBroker.Qpid;
using FFCEI.DotNetStack.MessageBroker.RabbitMQ;
using FFCEI.DotNetStack.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Reflection;

namespace FFCEI.DotNetStack.MessageBroker.Common;

internal sealed class MessageBrokerConnectorFactory
    : IMessageBrokerConnectorFactory, IMessageBrokerConnectorFactoryAutomaticReceiver
{
    private readonly ConcurrentDictionary<string, IMessageBrokerConnector> _connectors = new();
    private bool _disposed;

    internal MessageBrokerConnectorFactoryConfiguration Configuration { get; set; }
    internal ILoggerFactory? LoggerFactory { get; set; }
    public ILogger Logger { get; internal set; } = NullLogger.Instance;
    internal List<IReceiver> AutomaticReceivers { get; set; } = [];
    public Action<IMessageEnvelope>? OnUnknownMessageReceived { get; set; }
    public Action<IMessageEnvelope>? OnUnhandledMessage { get; set; }
    public Func<Exception, IMessageEnvelope?, bool>? OnMessageHandlingException { get; set; }

    public MessageBrokerConnectorFactory(MessageBrokerConnectorFactoryConfiguration configuration)
    {
        Configuration = configuration;
        Configuration.ValidateConfiguration();
    }

    private async Task<IMessageBrokerConnector> GetOrCreateConnectorAsync(MessageBrokerConnectionConfiguration connectionConfiguration, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var connectionId = connectionConfiguration.ConnectionId ?? string.Empty;

        var connector = await _connectors.GetOrAddValueTaskFunctionAsync(connectionId, async (_) =>
        {
            IMessageBrokerConnector connector = connectionConfiguration.ProviderType switch
            {
                MessageBrokerProviderType.Channels => new ChannelsConnector(LoggerFactory?.CreateLogger<ChannelsConnector>()),
                MessageBrokerProviderType.RabbitMQ => new RabbitMQConnector(LoggerFactory?.CreateLogger<RabbitMQConnector>()),
                MessageBrokerProviderType.Kafka => new KafkaConnector(LoggerFactory?.CreateLogger<KafkaConnector>()),
                MessageBrokerProviderType.Qpid => new QpidConnector(LoggerFactory?.CreateLogger<QpidConnector>()),
                MessageBrokerProviderType.AzureServiceBus => new AzureServiceBusConnector(LoggerFactory?.CreateLogger<AzureServiceBusConnector>()),
                MessageBrokerProviderType.GooglePubSub => new GooglePubSubConnector(LoggerFactory?.CreateLogger<GooglePubSubConnector>()),
                MessageBrokerProviderType.AmazonSQS => new AmazonSQSConnector(LoggerFactory?.CreateLogger<AmazonSQSConnector>()),
                MessageBrokerProviderType.AmazonSQSwithSNS => new AmazonSQSwithSNSConnector(LoggerFactory?.CreateLogger<AmazonSQSwithSNSConnector>()),
                _ => throw new NotSupportedException($"MessageBroker provider type '{connectionConfiguration.ProviderType}' is not supported")
            };

            if (connector is MessageBrokerConnector messageBrokerConnector)
            {
                messageBrokerConnector.SerializationAlgorithm = connectionConfiguration.SerializationAlgorithm;

                if (messageBrokerConnector.SerializationAlgorithm == null ||
                    messageBrokerConnector.SerializationAlgorithm == SerializationAlgorithm.Unspecified)
                {
                    messageBrokerConnector.SerializationAlgorithm = Configuration.SerializationAlgorithm;
                }
            }

            MessageBrokerLog.Connecting(Logger, connectionId, connectionConfiguration.ProviderType);

            try
            {
                await connector.StartAsync(connectionConfiguration.ConnectionString, cancellationToken).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                MessageBrokerLog.ConnectFailed(Logger, exception, connectionId, connectionConfiguration.ProviderType);

                throw;
            }

            MessageBrokerLog.Connected(Logger, connectionId, connectionConfiguration.ProviderType);

            await ApplyDeclarationsOnConnector(connectionConfiguration, _, connector, cancellationToken).ConfigureAwait(false);

            return connector;
        })
        .ConfigureAwait(false);

        return connector;
    }

    private static string ExchangeTypeFromAddressType(MessageBrokerAddressType exchangeType)
        => exchangeType switch
        {
            MessageBrokerAddressType.Direct => "direct",
            MessageBrokerAddressType.Topic => "topic",
            MessageBrokerAddressType.Fanout => "fanout",
            MessageBrokerAddressType.Headers => "headers",
            _ => throw new NotSupportedException($"Exchange type '{exchangeType}' is not supported")
        };

    private static async Task ApplyDeclarationsOnConnector(MessageBrokerConnectionConfiguration connectionConfiguration, string _, IMessageBrokerConnector connector, CancellationToken cancellationToken)
    {
        switch (connectionConfiguration.ProviderType)
        {
        case MessageBrokerProviderType.Channels:
            {
                if (connectionConfiguration.QueueDeclarations != null && connectionConfiguration.QueueDeclarations.Count > 0)
                {
                    throw new NotSupportedException($"MessageBroker provider type '{connectionConfiguration.ProviderType}' not support queue declarations");
                }

                if (connectionConfiguration.ExchangeDeclarations != null && connectionConfiguration.ExchangeDeclarations.Count > 0)
                {
                    throw new NotSupportedException($"MessageBroker provider type '{connectionConfiguration.ProviderType}' not support exchange declarations");
                }

                break;
            }
        case MessageBrokerProviderType.RabbitMQ:
            {
                var rabbitMQConnector = connector as RabbitMQConnector;

                foreach (var exchangeDeclaration in connectionConfiguration.ExchangeDeclarations ?? [])
                {
                    await rabbitMQConnector!.DeclareExchangeAsync(
                        exchangeDeclaration.ExchangeName,
                        ExchangeTypeFromAddressType(exchangeDeclaration.ExchangeType),
                        exchangeDeclaration.Durable,
                        exchangeDeclaration.AutoDelete,
                        exchangeDeclaration.Arguments?.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value),
                        cancellationToken).ConfigureAwait(false);
                }

                foreach (var queueDeclaration in connectionConfiguration.QueueDeclarations ?? [])
                {
                    _ = await rabbitMQConnector!.DeclareQueueAsync(
                        queueDeclaration.QueueName,
                        queueDeclaration.Durable,
                        queueDeclaration.Exclusive,
                        queueDeclaration.AutoDelete,
                        queueDeclaration.Arguments?.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value),
                        cancellationToken).ConfigureAwait(false);

                    foreach (var binding in queueDeclaration.Bindings ?? [])
                    {
                        AmqpCommons.ExtractExchangeAndRoutingKey(binding.BindingAddress, out var exchange, out var routingKey);

                        await rabbitMQConnector!.BindQueueToExchangeAsync(
                            queueDeclaration.QueueName,
                            exchange,
                            routingKey,
                            binding.Arguments?.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value),
                            cancellationToken).ConfigureAwait(false);
                    }
                }

                break;
            }
        case MessageBrokerProviderType.Kafka:
            {
                if (connectionConfiguration.ExchangeDeclarations != null && connectionConfiguration.ExchangeDeclarations.Count > 0)
                {
                    throw new NotSupportedException($"MessageBroker provider type '{connectionConfiguration.ProviderType}' not support exchange declarations");
                }

                var kafkaConnector = connector as KafkaConnector;

                foreach (var queueDeclaration in connectionConfiguration.QueueDeclarations ?? [])
                {
                    var topicSpecification = new TopicSpecification()
                    {
                        Configs = queueDeclaration.Arguments?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? [],
                        Name = queueDeclaration.QueueName,
                        NumPartitions = 1
                    };

                    await kafkaConnector!.CreateTopicAsync(
                        topicSpecification,
                        ignoreIfTopicAlreadyExists: true,
                        cancellationToken).ConfigureAwait(false);
                }

                break;
            }
        case MessageBrokerProviderType.Qpid:
            {
                var qpidConnector = connector as QpidConnector;

                foreach (var exchangeDeclaration in connectionConfiguration.ExchangeDeclarations ?? [])
                {
                    await qpidConnector!.DeclareExchangeAsync(
                        exchangeDeclaration.ExchangeName,
                        ExchangeTypeFromAddressType(exchangeDeclaration.ExchangeType),
                        exchangeDeclaration.Durable,
                        exchangeDeclaration.AutoDelete,
                        exchangeDeclaration.Arguments?.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value),
                        ignoreIfAlreadyExists: true,
                        cancellationToken).ConfigureAwait(false);
                }

                foreach (var queueDeclaration in connectionConfiguration.QueueDeclarations ?? [])
                {
                    await qpidConnector!.DeclareQueueAsync(
                        queueDeclaration.QueueName,
                        queueDeclaration.Durable,
                        queueDeclaration.Exclusive,
                        queueDeclaration.AutoDelete,
                        queueDeclaration.Arguments?.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value),
                        ignoreIfAlreadyExists: true,
                        cancellationToken).ConfigureAwait(false);

                    foreach (var binding in queueDeclaration.Bindings ?? [])
                    {
                        AmqpCommons.ExtractExchangeAndRoutingKey(binding.BindingAddress, out var exchange, out var routingKey);

                        await qpidConnector!.BindQueueToExchangeAsync(
                            queueDeclaration.QueueName,
                            exchange,
                            routingKey,
                            binding.Arguments?.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value),
                            ignoreIfAlreadyExists: true,
                            cancellationToken).ConfigureAwait(false);
                    }
                }

                break;
            }
        case MessageBrokerProviderType.AzureServiceBus:
            {
                if (connectionConfiguration.ExchangeDeclarations != null && connectionConfiguration.ExchangeDeclarations.Count > 0)
                {
                    throw new NotSupportedException($"MessageBroker provider type '{connectionConfiguration.ProviderType}' not support exchange declarations");
                }

                var azureServiceBusConnector = connector as AzureServiceBusConnector;

                foreach (var queueDeclaration in connectionConfiguration.QueueDeclarations ?? [])
                {
                    if ((queueDeclaration.Bindings == null) || (queueDeclaration.Bindings.Count == 0))
                    {
                        await azureServiceBusConnector!.DeclareQueueAsync(
                            queueDeclaration.QueueName,
                            ignoreIfAlreadyExists: true,
                            cancellationToken).ConfigureAwait(false);

                        continue;
                    }

                    foreach (var binding in queueDeclaration.Bindings)
                    {
                        var topic = AzureServiceBusConnector.ExtractEntityName(binding.BindingAddress);

                        await azureServiceBusConnector!.DeclareTopicAsync(
                            topic,
                            ignoreIfAlreadyExists: true,
                            cancellationToken).ConfigureAwait(false);

                        await azureServiceBusConnector!.DeclareSubscriptionAsync(
                            topic,
                            queueDeclaration.QueueName,
                            ignoreIfAlreadyExists: true,
                            cancellationToken).ConfigureAwait(false);
                    }
                }

                break;
            }
        case MessageBrokerProviderType.GooglePubSub:
            {
                if (connectionConfiguration.ExchangeDeclarations != null && connectionConfiguration.ExchangeDeclarations.Count > 0)
                {
                    throw new NotSupportedException($"MessageBroker provider type '{connectionConfiguration.ProviderType}' not support exchange declarations");
                }

                var googlePubSubConnector = connector as GooglePubSubConnector;

                foreach (var queueDeclaration in connectionConfiguration.QueueDeclarations ?? [])
                {
                    if ((queueDeclaration.Bindings == null) || (queueDeclaration.Bindings.Count == 0))
                    {
                        await googlePubSubConnector!.DeclareQueueAsync(
                            queueDeclaration.QueueName,
                            ignoreIfAlreadyExists: true,
                            cancellationToken).ConfigureAwait(false);

                        continue;
                    }

                    foreach (var binding in queueDeclaration.Bindings)
                    {
                        var topic = GooglePubSubConnector.ExtractTopicId(binding.BindingAddress);

                        await googlePubSubConnector!.DeclareTopicAsync(
                            topic,
                            ignoreIfAlreadyExists: true,
                            cancellationToken).ConfigureAwait(false);

                        await googlePubSubConnector!.DeclareSubscriptionAsync(
                            topic,
                            queueDeclaration.QueueName,
                            ignoreIfAlreadyExists: true,
                            cancellationToken).ConfigureAwait(false);
                    }
                }

                break;
            }
        case MessageBrokerProviderType.AmazonSQS:
            {
                if (connectionConfiguration.ExchangeDeclarations != null && connectionConfiguration.ExchangeDeclarations.Count > 0)
                {
                    throw new NotSupportedException($"MessageBroker provider type '{connectionConfiguration.ProviderType}' not support exchange declarations");
                }

                var amazonSQSConnector = connector as AmazonSQSConnector;

                foreach (var queueDeclaration in connectionConfiguration.QueueDeclarations ?? [])
                {
                    if ((queueDeclaration.Bindings != null) && (queueDeclaration.Bindings.Count > 0))
                    {
                        throw new NotSupportedException($"MessageBroker provider type '{connectionConfiguration.ProviderType}' not support queue declarations with bindings");
                    }

                    _ = await amazonSQSConnector!.DeclareQueueAsync(
                        queueDeclaration.QueueName,
                        queueDeclaration.Arguments?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                        cancellationToken).ConfigureAwait(false);
                }

                break;
            }
        case MessageBrokerProviderType.AmazonSQSwithSNS:
            {
                if (connectionConfiguration.ExchangeDeclarations != null && connectionConfiguration.ExchangeDeclarations.Count > 0)
                {
                    throw new NotSupportedException($"MessageBroker provider type '{connectionConfiguration.ProviderType}' not support exchange declarations");
                }

                var amazonSQSwithSNSConnector = connector as AmazonSQSwithSNSConnector;

                foreach (var queueDeclaration in connectionConfiguration.QueueDeclarations ?? [])
                {
                    if ((queueDeclaration.Bindings == null) || (queueDeclaration.Bindings.Count == 0))
                    {
                        _ = await amazonSQSwithSNSConnector!.DeclareQueueAsync(
                            queueDeclaration.QueueName,
                            queueDeclaration.Arguments?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                            cancellationToken).ConfigureAwait(false);

                        continue;
                    }

                    foreach (var binding in queueDeclaration.Bindings)
                    {
                        var topic = AmazonSQSwithSNSConnector.ExtractTopicName(binding.BindingAddress);

                        _ = await amazonSQSwithSNSConnector!.DeclareSubscriptionAsync(
                            topic,
                            queueDeclaration.QueueName,
                            queueDeclaration.Arguments?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                            cancellationToken).ConfigureAwait(false);
                    }
                }

                break;
            }
        default:
            {
                throw new NotSupportedException($"MessageBroker provider type '{connectionConfiguration.ProviderType}' is not supported");
            }
        }
    }

    private static Task<ISender> GetNewSenderAsync(SerializationAlgorithm? serializationAlgorithm, IMessageBrokerConnector connector, string? subject, MessageBrokerSubjectConfiguration? subjectConfiguration, MessageBrokerAddress? defaultAddress = null, CancellationToken cancellationToken = default)
    {
        if (serializationAlgorithm == null || serializationAlgorithm == SerializationAlgorithm.Unspecified)
        {
            serializationAlgorithm = subjectConfiguration?.SerializationAlgorithm;
        }

        if (serializationAlgorithm == null || serializationAlgorithm == SerializationAlgorithm.Unspecified)
        {
            serializationAlgorithm = (connector as MessageBrokerConnector)?.SerializationAlgorithm;
        }

        var serializer = SerializationConfiguration.SerializerFrom(serializationAlgorithm);

        subject ??= string.Empty;

        return connector.GetSenderAsync(subject, defaultAddress, serializer, cancellationToken);
    }

    private MessageBrokerSubjectConfiguration? GetSubjectConfiguration(string subject)
    {
        var subjectConfiguration = Configuration.Subjects?.FirstOrDefault(existingSubject => existingSubject.Name == subject);

        if (subjectConfiguration == null)
        {
            if (Configuration.HasSubjects)
            {
                throw new MessageBrokerConnectorFactoryException($"Unknown MessageBroker subject '{subject}'");
            }
        }

        return subjectConfiguration;
    }

    private async Task<ISender> DoGetSenderAsync(SerializationAlgorithm? serializationAlgorithm, string subject, MessageBrokerAddress? defaultAddress, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        ArgumentException.ThrowIfNullOrWhiteSpace(subject, nameof(subject));

        var subjectConfiguration = GetSubjectConfiguration(subject);
        var connector = await GetConnectorAsync(subject, cancellationToken).ConfigureAwait(false);
        var sender = await GetNewSenderAsync(serializationAlgorithm, connector, subject, subjectConfiguration, defaultAddress, cancellationToken).ConfigureAwait(false);

        return sender;
    }

    private async Task<ISender> DoGetDefaultSenderAsync(SerializationAlgorithm? serializationAlgorithm, MessageBrokerAddress? defaultAddress, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var subjectConfiguration = Configuration.DefaultSubject;
        var connector = await GetDefaultConnectorAsync(cancellationToken).ConfigureAwait(false);
        var sender = await GetNewSenderAsync(serializationAlgorithm, connector, string.Empty, subjectConfiguration, defaultAddress, cancellationToken).ConfigureAwait(false);

        return sender;
    }

    private async Task<IReceiver> GetNewReceiverAsync(SerializationAlgorithm? serializationAlgorithm, IMessageBrokerConnector connector, string? subject, MessageBrokerSubjectConfiguration? subjectConfiguration, CancellationToken cancellationToken = default)
    {
        if (serializationAlgorithm == null || serializationAlgorithm == SerializationAlgorithm.Unspecified)
        {
            serializationAlgorithm = subjectConfiguration?.SerializationAlgorithm;
        }

        if (serializationAlgorithm == null || serializationAlgorithm == SerializationAlgorithm.Unspecified)
        {
            serializationAlgorithm = (connector as MessageBrokerConnector)?.SerializationAlgorithm;
        }

        var deserializer = SerializationConfiguration.DeserializerFrom(serializationAlgorithm);

        subject ??= string.Empty;

        var receiver = await connector.GetReceiverAsync(subject, deserializer, cancellationToken).ConfigureAwait(false);

        receiver.OnUnknownMessageReceived = OnUnknownMessageReceived;
        receiver.OnUnhandledMessage = OnUnhandledMessage;
        receiver.OnMessageHandlingException = OnMessageHandlingException;

        return receiver;
    }

    private async Task<IReceiver> DoGetReceiverAsync(SerializationAlgorithm? serializationAlgorithm, string subject, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject, nameof(subject));

        var subjectConfiguration = GetSubjectConfiguration(subject);
        var connector = await GetConnectorAsync(subject, cancellationToken).ConfigureAwait(false);
        var receiver = await GetNewReceiverAsync(serializationAlgorithm, connector, subject, subjectConfiguration, cancellationToken).ConfigureAwait(false);

        return receiver;
    }

    private async Task<IReceiver> DoGetDefaultReceiverAsync(SerializationAlgorithm? serializationAlgorithm, CancellationToken cancellationToken)
    {
        var subjectConfiguration = Configuration.DefaultSubject;
        var connector = await GetDefaultConnectorAsync(cancellationToken).ConfigureAwait(false);
        var receiver = await GetNewReceiverAsync(serializationAlgorithm, connector, string.Empty, subjectConfiguration, cancellationToken).ConfigureAwait(false);

        return receiver;
    }

    public IMessageHandlerCallback ConfigureCallbacks(Action<IMessageHandlerCallback> expression)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        expression(this);

        return this;
    }

    public Task<IMessageBrokerConnector> GetConnectorByConnectionIdAsync(string? connectionId, CancellationToken cancellationToken = default)
    {
        var connectionConfiguration = (string.IsNullOrWhiteSpace(connectionId) ?
            Configuration.DefaultConnection :
            Configuration.Connections.FirstOrDefault(connection => connection.ConnectionId == connectionId)) ??
            throw new MessageBrokerConnectorFactoryException($"MessageBroker connection '{connectionId}' not found");

        return GetOrCreateConnectorAsync(connectionConfiguration, cancellationToken);
    }

    public Task<IMessageBrokerConnector> GetConnectorAsync(string subject, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject, nameof(subject));

        var subjectConfiguration = GetSubjectConfiguration(subject);
        var connectionId = subjectConfiguration?.ConnectionId;

        return GetConnectorByConnectionIdAsync(connectionId, cancellationToken);
    }

    public Task<IMessageBrokerConnector> GetDefaultConnectorAsync(CancellationToken cancellationToken = default)
    {
        var connectionConfiguration = Configuration.DefaultConnection ??
            throw new MessageBrokerConnectorFactoryException("No default MessageBroker connection is configured");

        return GetOrCreateConnectorAsync(connectionConfiguration, cancellationToken);
    }

    public Task<ISender> GetSenderAsync(string subject, MessageBrokerAddress? defaultAddress = null, CancellationToken cancellationToken = default)
        => DoGetSenderAsync(null, subject, defaultAddress, cancellationToken);

    public Task<ISender> GetDefaultSenderAsync(MessageBrokerAddress? defaultAddress = null, CancellationToken cancellationToken = default)
        => DoGetDefaultSenderAsync(null, defaultAddress, cancellationToken);

    public Task<IReceiver> GetReceiverAsync(string subject, CancellationToken cancellationToken = default)
        => DoGetReceiverAsync(null, subject, cancellationToken);

    public Task<IReceiver> GetDefaultReceiverAsync(CancellationToken cancellationToken = default)
        => DoGetDefaultReceiverAsync(null, cancellationToken);

    public async Task StartAutomaticReceiversAsync(IEnumerable<Assembly> assembliesToLoadGlobalHandlers, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (AutomaticReceivers.Count > 0)
        {
            throw new InvalidOperationException("Automatic Receivers Already Started");
        }

        foreach (var assembly in assembliesToLoadGlobalHandlers)
        {
            _ = MessageHandlerRegistry.Global.LoadAssemblyHandlerTypes(assembly);
        }

        var configuredReceivers = new List<IReceiver>();

        try
        {
            foreach (var automaticReceiver in Configuration.AutomaticReceivers ?? [])
            {
                var receiver = (string.IsNullOrWhiteSpace(automaticReceiver.Subject) ?
                    await DoGetDefaultReceiverAsync(automaticReceiver.SerializationAlgorithm, cancellationToken).ConfigureAwait(false) :
                    await DoGetReceiverAsync(automaticReceiver.SerializationAlgorithm, automaticReceiver.Subject, cancellationToken).ConfigureAwait(false))
                    ?? throw new InvalidOperationException($"Failed to create automatic receiver for subject '{automaticReceiver.Subject}'");

                if (receiver is not IConfigurableReceiver configurableReceiver)
                {
                    throw new InvalidCastException($"Receiver of type '{receiver.GetType()}' does not implement IConfigurableReceiver");
                }

                await configurableReceiver.ConfigureAsync(automaticReceiver.Address, cancellationToken).ConfigureAwait(false);

                foreach (var handler in automaticReceiver.Handlers ?? [])
                {
                    var handlerType = MessageHandlerRegistryExtensionMethods.LoadHandler(handler) ??
                        throw new InvalidOperationException($"Failed to locate automatic receiver handler type '{handler}'");

                    if (Activator.CreateInstance(handlerType) is not IMessageEnvelopeHandler receiverHandler)
                    {
                        throw new InvalidOperationException($"Failed to create automatic receiver handler of type '{handler}'");
                    }

                    receiver.AddMessageHandler(receiverHandler);
                }

                configuredReceivers.Add(receiver);
            }

            foreach (var configuredReceiver in configuredReceivers)
            {
                await configuredReceiver.StartAsync(cancellationToken).ConfigureAwait(false);
            }

            AutomaticReceivers.AddRange(configuredReceivers);
        }
        catch
        {
            foreach (var configuredReceiver in configuredReceivers)
            {
                await ExceptionProtector.ExecuteTaskAsync(configuredReceiver.StopAsync()).ConfigureAwait(false);

                await configuredReceiver.DisposeSafelyIfNeededAsync().ConfigureAwait(false);
            }

            throw;
        }
    }

    public Task StartAutomaticReceiversAsync(CancellationToken cancellationToken = default)
        => StartAutomaticReceiversAsync([], cancellationToken);

    public async ValueTask DisposeAsync()
    {
        foreach (var connector in _connectors.Values)
        {
            await connector.DisposeSafelyIfNeededAsync().ConfigureAwait(false);
        }

        _connectors.Clear();

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        var _ = ExceptionProtector.ExecuteValueTaskAsync(DisposeAsync());

        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
