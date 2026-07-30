using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using FFCEI.DotNetStack.Serialization.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FFCEI.DotNetStack.MessageBroker.AzureServiceBus;

public sealed class AzureServiceBusConnector(ILogger<AzureServiceBusConnector>? logger = null)
        : MessageBrokerConnector(MessageBrokerProviderType.AzureServiceBus, logger)
{
#pragma warning disable CA2213 // Disposable fields should be disposed
    private ServiceBusClient? _client;
#pragma warning restore CA2213 // Disposable fields should be disposed
    private ServiceBusAdministrationClient? _administrationClient;
    private readonly ConcurrentDictionary<ServiceBusSender, object> _senderLinks = [];
    private readonly ConcurrentDictionary<ServiceBusReceiver, object> _receiverLinks = [];
    private readonly ConcurrentDictionary<WeakReference<AzureServiceBusReceiver>, object> _receivers = [];

    internal ServiceBusClient Client => _client ??
        throw new InvalidOperationException("AzureServiceBusConnector not started");

    internal ServiceBusAdministrationClient AdministrationClient => _administrationClient ?? throw new InvalidOperationException("AzureServiceBusConnector not started");

    public override bool Started()
        => !Disposed && (_client != null);

    private void ThrowIfAlreadyStarted()
    {
        if (_client != null)
        {
            throw new InvalidOperationException("AzureServiceBusConnector already started");
        }
    }

    public Task StartAsync(ServiceBusClient client, ServiceBusAdministrationClient? administrationClient = null)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ArgumentNullException.ThrowIfNull(client, nameof(client));

        ThrowIfAlreadyStarted();

        _client = client;
        _administrationClient = administrationClient;

        MessageBrokerLog.ConnectorStarted(Logger, MessageBrokerProvider);

        return Task.CompletedTask;
    }

    public override Task StartAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri, nameof(uri));

        return StartAsync(uri.OriginalString, cancellationToken);
    }

    public override Task StartAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

        ThrowIfAlreadyStarted();

        try
        {
            _client = new(connectionString);
            _administrationClient = new(connectionString);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            MessageBrokerLog.ConnectorStartFailed(Logger, exception, MessageBrokerProvider);

            throw;
        }

        MessageBrokerLog.ConnectorStarted(Logger, MessageBrokerProvider);

        return Task.CompletedTask;
    }

    public override async Task StopAsync()
    {
        foreach (var weakReceiver in _receivers.Keys)
        {
            if (weakReceiver.TryGetTarget(out var receiver) && (receiver != null))
            {
                await ExceptionProtector.ExecuteTaskAsync(receiver.StopAsync()).ConfigureAwait(false);
            }
        }

        _receivers.Clear();

        foreach (var receiverLink in _receiverLinks.Keys)
        {
            await ExceptionProtector.ExecuteTaskAsync(receiverLink.CloseAsync()).ConfigureAwait(false);

            await receiverLink.DisposeSafelyIfNeededAsync().ConfigureAwait(false);
        }

        _receiverLinks.Clear();

        foreach (var senderLink in _senderLinks.Keys)
        {
            await ExceptionProtector.ExecuteTaskAsync(senderLink.CloseAsync()).ConfigureAwait(false);

            await senderLink.DisposeSafelyIfNeededAsync().ConfigureAwait(false);
        }

        _senderLinks.Clear();

        if (_client != null)
        {
            await _client.DisposeSafelyIfNeededAsync().ConfigureAwait(false);

            _client = null;
        }

        _administrationClient = null;
    }

    public override Task<ISender> GetSenderAsync(string subject, MessageBrokerAddress? defaultAddress = null, ISerializer? serializer = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        _ = Client;

        var sender = new AzureServiceBusSender(subject, new WeakReference<AzureServiceBusConnector>(this))
        {
            DefaultAddress = defaultAddress,
            Serializer = serializer
        };

        MessageBrokerLog.SenderCreated(Logger, MessageBrokerProvider, subject);

        return Task.FromResult<ISender>(sender);
    }

    public override Task<IReceiver> GetReceiverAsync(string subject, IDeserializer? deserializer = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        _ = Client;

        var receiver = new AzureServiceBusReceiver(subject, new WeakReference<AzureServiceBusConnector>(this))
        {
            Deserializer = deserializer
        };

        _receivers[new WeakReference<AzureServiceBusReceiver>(receiver)] = true;

        MessageBrokerLog.ReceiverCreated(Logger, MessageBrokerProvider, subject);

        return Task.FromResult<IReceiver>(receiver);
    }

    protected override void OnDispose()
        => _client.DisposeObjectSafelyIfNeeded();

    protected override ValueTask OnDisposeAsync()
        => _client.DisposeSafelyIfNeededAsync();

    internal ServiceBusSender OpenSenderLink(string entityName)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        var senderLink = Client.CreateSender(entityName);

        _senderLinks[senderLink] = true;

        return senderLink;
    }

    internal async Task CloseSenderLinkAsync(ServiceBusSender senderLink)
    {
        if (_senderLinks.TryRemove(senderLink, out var _))
        {
            await ExceptionProtector.ExecuteTaskAsync(senderLink.CloseAsync()).ConfigureAwait(false);

            await senderLink.DisposeSafelyIfNeededAsync().ConfigureAwait(false);
        }
    }

    internal ServiceBusReceiver OpenReceiverLink(MessageBrokerAddress address, string subject)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        var entityName = ExtractEntityName(address);

        var receiverOptions = new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        };

        var receiverLink = (address.Type is MessageBrokerAddressType.Topic or MessageBrokerAddressType.Fanout) ?
            Client.CreateReceiver(entityName, string.IsNullOrWhiteSpace(subject) ? "default" : subject, receiverOptions) :
            Client.CreateReceiver(entityName, receiverOptions);

        _receiverLinks[receiverLink] = true;

        return receiverLink;
    }

    internal async Task CloseReceiverLinkAsync(ServiceBusReceiver receiverLink)
    {
        if (_receiverLinks.TryRemove(receiverLink, out var _))
        {
            await ExceptionProtector.ExecuteTaskAsync(receiverLink.CloseAsync()).ConfigureAwait(false);

            await receiverLink.DisposeSafelyIfNeededAsync().ConfigureAwait(false);
        }
    }

    public async Task DeclareQueueAsync(string queue, bool ignoreIfAlreadyExists = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queue, nameof(queue));

        try
        {
            _ = await AdministrationClient.CreateQueueAsync(queue, cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.Declared(Logger, MessageBrokerProvider, "queue", queue);
        }
        catch (ServiceBusException exception) when (ignoreIfAlreadyExists && (exception.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists))
        {
        }
        catch (ServiceBusException exception)
        {
            MessageBrokerLog.DeclareFailed(Logger, exception, MessageBrokerProvider, "queue", queue);

            throw;
        }
    }

    public async Task DeclareTopicAsync(string topic, bool ignoreIfAlreadyExists = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic, nameof(topic));

        try
        {
            _ = await AdministrationClient.CreateTopicAsync(topic, cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.Declared(Logger, MessageBrokerProvider, "topic", topic);
        }
        catch (ServiceBusException exception) when (ignoreIfAlreadyExists && (exception.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists))
        {
        }
        catch (ServiceBusException exception)
        {
            MessageBrokerLog.DeclareFailed(Logger, exception, MessageBrokerProvider, "topic", topic);

            throw;
        }
    }

    public async Task DeclareSubscriptionAsync(string topic, string subscription, bool ignoreIfAlreadyExists = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic, nameof(topic));
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription, nameof(subscription));

        try
        {
            _ = await AdministrationClient.CreateSubscriptionAsync(topic, subscription, cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.DeclaredSubscription(Logger, MessageBrokerProvider, subscription, topic);
        }
        catch (ServiceBusException exception) when (ignoreIfAlreadyExists && (exception.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists))
        {
        }
        catch (ServiceBusException exception)
        {
            MessageBrokerLog.DeclareSubscriptionFailed(Logger, exception, MessageBrokerProvider, subscription, topic);

            throw;
        }
    }

    public async Task DeleteQueueAsync(string queue, bool ignoreIfDoesNotExist = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queue, nameof(queue));

        try
        {
            _ = await AdministrationClient.DeleteQueueAsync(queue, cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.Deleted(Logger, MessageBrokerProvider, "queue", queue);
        }
        catch (ServiceBusException exception) when (ignoreIfDoesNotExist && (exception.Reason == ServiceBusFailureReason.MessagingEntityNotFound))
        {
        }
        catch (ServiceBusException exception)
        {
            MessageBrokerLog.DeleteFailed(Logger, exception, MessageBrokerProvider, "queue", queue);

            throw;
        }
    }

    public async Task DeleteTopicAsync(string topic, bool ignoreIfDoesNotExist = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic, nameof(topic));

        try
        {
            _ = await AdministrationClient.DeleteTopicAsync(topic, cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.Deleted(Logger, MessageBrokerProvider, "topic", topic);
        }
        catch (ServiceBusException exception) when (ignoreIfDoesNotExist && (exception.Reason == ServiceBusFailureReason.MessagingEntityNotFound))
        {
        }
        catch (ServiceBusException exception)
        {
            MessageBrokerLog.DeleteFailed(Logger, exception, MessageBrokerProvider, "topic", topic);

            throw;
        }
    }

    public async Task DeleteSubscriptionAsync(string topic, string subscription, bool ignoreIfDoesNotExist = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic, nameof(topic));
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription, nameof(subscription));

        try
        {
            _ = await AdministrationClient.DeleteSubscriptionAsync(topic, subscription, cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.DeletedSubscription(Logger, MessageBrokerProvider, subscription, topic);
        }
        catch (ServiceBusException exception) when (ignoreIfDoesNotExist && (exception.Reason == ServiceBusFailureReason.MessagingEntityNotFound))
        {
        }
        catch (ServiceBusException exception)
        {
            MessageBrokerLog.DeleteSubscriptionFailed(Logger, exception, MessageBrokerProvider, subscription, topic);

            throw;
        }
    }

    internal static string ExtractEntityName(MessageBrokerAddress address)
        => address.Type switch
        {
            MessageBrokerAddressType.Direct => address.Value,
            MessageBrokerAddressType.Queue => address.Value,
            MessageBrokerAddressType.Topic => address.Value,
            MessageBrokerAddressType.Fanout => address.Value,
            _ => throw new NotSupportedException($"Address type '{address.Type}' is not supported by Azure Service Bus")
        };

    internal static ServiceBusMessage MessageAttributesToServiceBusMessage(MessageEnvelope messageEnvelope)
    {
        var sentTimestamp = messageEnvelope.Attributes.TouchSentTimestamp();

        var message = new ServiceBusMessage(new BinaryData(messageEnvelope.PayloadBytes ?? []))
        {
            ContentType = Constants.ContentTypeValue,
            MessageId = ValueConversions.GuidToString(messageEnvelope.Attributes.MessageId)
        };

        message.ApplicationProperties[Constants.ContentEncodingHeaderKey] = Constants.ContentEncodingValue;

        if (messageEnvelope.Attributes.PayloadType != null)
        {
            message.ApplicationProperties[Constants.PayloadTypeHeaderKey] = messageEnvelope.Attributes.PayloadType;
        }

        message.ApplicationProperties[Constants.SendTimestampHeaderKey] = ValueConversions.DateTimeOffsetToString(sentTimestamp);

        if (messageEnvelope.Attributes.TraceParent != null)
        {
            message.ApplicationProperties[Constants.TraceParentHeaderKey] = messageEnvelope.Attributes.TraceParent;
        }

        if (messageEnvelope.Attributes.TraceState != null)
        {
            message.ApplicationProperties[Constants.TraceStateHeaderKey] = messageEnvelope.Attributes.TraceState;
        }

        if (messageEnvelope.Attributes.Baggage != null)
        {
            message.ApplicationProperties[Constants.BaggageHeaderKey] = messageEnvelope.Attributes.Baggage;
        }

        if (messageEnvelope.Attributes.TTL.HasValue)
        {
            message.TimeToLive = messageEnvelope.Attributes.TTL.Value;
        }

        return message;
    }

    internal static MessageEnvelopeAttributes MessageAttributesFromServiceBusMessage(ServiceBusReceivedMessage message)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        try
        {
            var result = new MessageEnvelopeAttributes
            {
                TraceParent = message.ApplicationProperties.TryGetValue(Constants.TraceParentHeaderKey, out var traceParentValue) ? traceParentValue?.ToString() : null,
                TraceState = message.ApplicationProperties.TryGetValue(Constants.TraceStateHeaderKey, out var traceStateValue) ? traceStateValue?.ToString() : null,
                Baggage = message.ApplicationProperties.TryGetValue(Constants.BaggageHeaderKey, out var baggageValue) ? baggageValue?.ToString() : null,
                RoutingHeaders = MessageEnvelopeAttributes.EmptyRoutingHeaders,
                TTL = (message.TimeToLive.Ticks == 0) || (message.TimeToLive == TimeSpan.MaxValue) ? null : message.TimeToLive,
                Durable = false,
                MessageId = ValueConversions.TryGuidFromString(message.MessageId) ?? Guid.CreateVersion7(),
                SentTimestamp = ValueConversions.TryDateTimeOffsetFromString(message.ApplicationProperties.TryGetValue(Constants.SendTimestampHeaderKey, out var sendTimestampValue) ? sendTimestampValue?.ToString() : null),
                PayloadType = message.ApplicationProperties.TryGetValue(Constants.PayloadTypeHeaderKey, out var payloadTypeValue) ? payloadTypeValue?.ToString() : null,
            }
            .FixTimestamps();

            return result;
        }
        catch (Exception exception)
        {
            throw new DeserializeMessageEnvelopeException(exception);
        }
    }
}
