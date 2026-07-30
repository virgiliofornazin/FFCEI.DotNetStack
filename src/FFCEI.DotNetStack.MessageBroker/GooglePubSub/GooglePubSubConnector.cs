using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using FFCEI.DotNetStack.Serialization;
using FFCEI.DotNetStack.Serialization.Interfaces;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

namespace FFCEI.DotNetStack.MessageBroker.GooglePubSub;

public sealed class GooglePubSubConnector(ILogger<GooglePubSubConnector>? logger = null)
        : MessageBrokerConnector(MessageBrokerProviderType.GooglePubSub, logger)
{
    private string? _projectId;
    private PublisherServiceApiClient? _publisherApi;
    private SubscriberServiceApiClient? _subscriberApi;
    private readonly ConcurrentDictionary<WeakReference<GooglePubSubReceiver>, object> _receivers = [];

    internal string ProjectId => _projectId ??
        throw new InvalidOperationException("GooglePubSubConnector not started");

    internal PublisherServiceApiClient PublisherApi => _publisherApi ??
        throw new InvalidOperationException("GooglePubSubConnector not started");

    internal SubscriberServiceApiClient SubscriberApi => _subscriberApi ??
        throw new InvalidOperationException("GooglePubSubConnector not started");

    public override bool Started()
        => !Disposed && (_publisherApi != null);

    private void ThrowIfAlreadyStarted()
    {
        if (_publisherApi != null)
        {
            throw new InvalidOperationException("GooglePubSubConnector already started");
        }
    }

    public override Task StartAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri, nameof(uri));

        return StartAsync(uri.OriginalString, cancellationToken);
    }

    public override async Task StartAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

        ThrowIfAlreadyStarted();

        var projectId = connectionString.Trim();

        if (projectId.StartsWith('{'))
        {
            Dictionary<string, string> settings;

            try
            {
                settings = JsonDeserializer.Default.Deserialize<Dictionary<string, string>>(projectId)?
                    .ToDictionary(result => result.Key.ToUpperInvariant(), result => result.Value) ??
                        throw new ArgumentException("Connection string is not a valid JSON settings object", nameof(connectionString));
            }
            catch (JsonException exception)
            {
                throw new ArgumentException("Connection string is not a valid JSON settings object", nameof(connectionString), exception);
            }

            projectId = settings.TryGetValue("PROJECTID", out var projectIdValue) && !string.IsNullOrWhiteSpace(projectIdValue) ?
                projectIdValue :
                throw new ArgumentException("JSON settings must define a 'ProjectId' entry", nameof(connectionString));
        }

        _projectId = projectId;

        try
        {
            _publisherApi = await PublisherServiceApiClient.CreateAsync(cancellationToken).ConfigureAwait(false);
            _subscriberApi = await SubscriberServiceApiClient.CreateAsync(cancellationToken).ConfigureAwait(false);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            MessageBrokerLog.ConnectorStartFailed(Logger, exception, MessageBrokerProvider);

            throw;
        }

        MessageBrokerLog.ConnectorStarted(Logger, MessageBrokerProvider);
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

        _publisherApi = null;
        _subscriberApi = null;
        _projectId = null;
    }

    public override Task<ISender> GetSenderAsync(string subject, MessageBrokerAddress? defaultAddress = null, ISerializer? serializer = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        _ = PublisherApi;

        var sender = new GooglePubSubSender(subject, new WeakReference<GooglePubSubConnector>(this))
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

        _ = SubscriberApi;

        var receiver = new GooglePubSubReceiver(subject, new WeakReference<GooglePubSubConnector>(this))
        {
            Deserializer = deserializer
        };

        _receivers[new WeakReference<GooglePubSubReceiver>(receiver)] = true;

        MessageBrokerLog.ReceiverCreated(Logger, MessageBrokerProvider, subject);

        return Task.FromResult<IReceiver>(receiver);
    }

    internal async Task<bool> HasDeadLetterPolicyAsync(SubscriptionName subscriptionName, CancellationToken cancellationToken)
    {
        var subscription = await SubscriberApi.GetSubscriptionAsync(subscriptionName, cancellationToken).ConfigureAwait(false);

        return subscription.DeadLetterPolicy != null;
    }

    internal TopicName GetTopicName(MessageBrokerAddress address)
        => TopicName.FromProjectTopic(ProjectId, ExtractTopicId(address));

    internal SubscriptionName GetSubscriptionName(MessageBrokerAddress address, string subject)
        => SubscriptionName.FromProjectSubscription(ProjectId, address.Type switch
        {
            MessageBrokerAddressType.Direct => address.Value,
            MessageBrokerAddressType.Queue => address.Value,
            MessageBrokerAddressType.Topic => $"{address.Value}-{(string.IsNullOrWhiteSpace(subject) ? "default" : subject)}",
            MessageBrokerAddressType.Fanout => $"{address.Value}-{(string.IsNullOrWhiteSpace(subject) ? "default" : subject)}",
            _ => throw new NotSupportedException($"Address type '{address.Type}' is not supported by Google Pub/Sub")
        });

    internal static string ExtractTopicId(MessageBrokerAddress address)
        => address.Type switch
        {
            MessageBrokerAddressType.Direct => address.Value,
            MessageBrokerAddressType.Queue => address.Value,
            MessageBrokerAddressType.Topic => address.Value,
            MessageBrokerAddressType.Fanout => address.Value,
            _ => throw new NotSupportedException($"Address type '{address.Type}' is not supported by Google Pub/Sub")
        };

    public async Task DeclareTopicAsync(string topic, bool ignoreIfAlreadyExists = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic, nameof(topic));

        try
        {
            _ = await PublisherApi.CreateTopicAsync(TopicName.FromProjectTopic(ProjectId, topic), cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.Declared(Logger, MessageBrokerProvider, "topic", topic);
        }
        catch (RpcException exception) when (ignoreIfAlreadyExists && (exception.StatusCode == StatusCode.AlreadyExists))
        {
        }
        catch (RpcException exception)
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
            var request = new Subscription
            {
                SubscriptionName = SubscriptionName.FromProjectSubscription(ProjectId, subscription),
                TopicAsTopicName = TopicName.FromProjectTopic(ProjectId, topic),
                AckDeadlineSeconds = 60
            };

            _ = await SubscriberApi.CreateSubscriptionAsync(request, cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.DeclaredSubscription(Logger, MessageBrokerProvider, subscription, topic);
        }
        catch (RpcException exception) when (ignoreIfAlreadyExists && (exception.StatusCode == StatusCode.AlreadyExists))
        {
        }
        catch (RpcException exception)
        {
            MessageBrokerLog.DeclareSubscriptionFailed(Logger, exception, MessageBrokerProvider, subscription, topic);

            throw;
        }
    }

    public async Task DeclareQueueAsync(string queue, bool ignoreIfAlreadyExists = true, CancellationToken cancellationToken = default)
    {
        await DeclareTopicAsync(queue, ignoreIfAlreadyExists, cancellationToken).ConfigureAwait(false);
        await DeclareSubscriptionAsync(queue, queue, ignoreIfAlreadyExists, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteTopicAsync(string topic, bool ignoreIfDoesNotExist = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic, nameof(topic));

        try
        {
            await PublisherApi.DeleteTopicAsync(TopicName.FromProjectTopic(ProjectId, topic), cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.Deleted(Logger, MessageBrokerProvider, "topic", topic);
        }
        catch (RpcException exception) when (ignoreIfDoesNotExist && (exception.StatusCode == StatusCode.NotFound))
        {
        }
        catch (RpcException exception)
        {
            MessageBrokerLog.DeleteFailed(Logger, exception, MessageBrokerProvider, "topic", topic);

            throw;
        }
    }

    public async Task DeleteSubscriptionAsync(string subscription, bool ignoreIfDoesNotExist = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription, nameof(subscription));

        try
        {
            await SubscriberApi.DeleteSubscriptionAsync(SubscriptionName.FromProjectSubscription(ProjectId, subscription), cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.Deleted(Logger, MessageBrokerProvider, "subscription", subscription);
        }
        catch (RpcException exception) when (ignoreIfDoesNotExist && (exception.StatusCode == StatusCode.NotFound))
        {
        }
        catch (RpcException exception)
        {
            MessageBrokerLog.DeleteFailed(Logger, exception, MessageBrokerProvider, "subscription", subscription);

            throw;
        }
    }

    public async Task DeleteQueueAsync(string queue, bool ignoreIfDoesNotExist = true, CancellationToken cancellationToken = default)
    {
        await DeleteSubscriptionAsync(queue, ignoreIfDoesNotExist, cancellationToken).ConfigureAwait(false);
        await DeleteTopicAsync(queue, ignoreIfDoesNotExist, cancellationToken).ConfigureAwait(false);
    }

    internal static PubsubMessage MessageAttributesToPubsubMessage(MessageEnvelope messageEnvelope)
    {
        var sentTimestamp = messageEnvelope.Attributes.TouchSentTimestamp();

        var message = new PubsubMessage
        {
            Data = ByteString.CopyFrom(messageEnvelope.PayloadBytes ?? [])
        };

        message.Attributes[Constants.ContentTypeHeaderKey] = Constants.ContentTypeValue;
        message.Attributes[Constants.ContentEncodingHeaderKey] = Constants.ContentEncodingValue;

        if (messageEnvelope.Attributes.PayloadType != null)
        {
            message.Attributes[Constants.PayloadTypeHeaderKey] = messageEnvelope.Attributes.PayloadType;
        }

        message.Attributes[Constants.MessageIdHeaderKey] = ValueConversions.GuidToString(messageEnvelope.Attributes.MessageId);
        message.Attributes[Constants.SendTimestampHeaderKey] = ValueConversions.DateTimeOffsetToString(sentTimestamp);

        if (messageEnvelope.Attributes.TraceParent != null)
        {
            message.Attributes[Constants.TraceParentHeaderKey] = messageEnvelope.Attributes.TraceParent;
        }

        if (messageEnvelope.Attributes.TraceState != null)
        {
            message.Attributes[Constants.TraceStateHeaderKey] = messageEnvelope.Attributes.TraceState;
        }

        if (messageEnvelope.Attributes.Baggage != null)
        {
            message.Attributes[Constants.BaggageHeaderKey] = messageEnvelope.Attributes.Baggage;
        }

        if (messageEnvelope.Attributes.TTL.HasValue)
        {
            message.Attributes[Constants.TTLHeaderKey] = ((long)messageEnvelope.Attributes.TTL.Value.TotalMilliseconds).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return message;
    }

    internal static MessageEnvelopeAttributes MessageAttributesFromPubsubMessage(PubsubMessage message)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        try
        {
            var result = new MessageEnvelopeAttributes
            {
                TraceParent = GetAttributeStringOrDefault(message, Constants.TraceParentHeaderKey),
                TraceState = GetAttributeStringOrDefault(message, Constants.TraceStateHeaderKey),
                Baggage = GetAttributeStringOrDefault(message, Constants.BaggageHeaderKey),
                RoutingHeaders = MessageEnvelopeAttributes.EmptyRoutingHeaders,
                TTL = TryGetHeaderString(message, Constants.TTLHeaderKey, out var ttlText) &&
                    !string.IsNullOrWhiteSpace(ttlText) ? TimeSpan.FromMilliseconds(long.Parse(ttlText, CultureInfo.InvariantCulture)) : null,
                Durable = false,
                MessageId = ValueConversions.TryGuidFromString(GetAttributeStringOrDefault(message, Constants.MessageIdHeaderKey)) ?? Guid.CreateVersion7(),
                SentTimestamp = ValueConversions.TryDateTimeOffsetFromString(GetAttributeStringOrDefault(message, Constants.SendTimestampHeaderKey)),
                PayloadType = GetAttributeStringOrDefault(message, Constants.PayloadTypeHeaderKey),
            }
            .FixTimestamps();

            return result;
        }
        catch (Exception exception)
        {
            throw new DeserializeMessageEnvelopeException(exception);
        }
    }

    internal static bool TryGetHeaderString(PubsubMessage message, string key, out string? value)
        => message.Attributes.TryGetValue(key, out value);

    internal static string? GetAttributeStringOrDefault(PubsubMessage message, string key, string? defaultValue = null)
        => TryGetHeaderString(message, key, out var value) ? value : defaultValue;

}
