using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using FFCEI.DotNetStack.Serialization;
using FFCEI.DotNetStack.Serialization.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

namespace FFCEI.DotNetStack.MessageBroker.AmazonSQS;

public sealed class AmazonSQSConnector(ILogger<AmazonSQSConnector>? logger = null)
        : MessageBrokerConnector(MessageBrokerProviderType.AmazonSQS, logger)
{
    private IAmazonSQS? _client;
    private readonly ConcurrentDictionary<string, string> _queueUrls = [];
    private readonly ConcurrentDictionary<WeakReference<AmazonSQSReceiver>, object> _receivers = [];

    internal IAmazonSQS Client => _client != null ? _client! :
        throw new InvalidOperationException("AmazonSQSConnector not started");

    public override bool Started()
        => !Disposed && (_client != null);

    private void ThrowIfAlreadyStarted()
    {
        if (_client != null)
        {
            throw new InvalidOperationException("AmazonSQSConnector already started");
        }
    }

    private void SetClient(Func<IAmazonSQS> factory)
    {
        try
        {
            _client = factory();
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

    public Task StartAsync(IAmazonSQS client)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ArgumentNullException.ThrowIfNull(client, nameof(client));

        ThrowIfAlreadyStarted();

        _client = client;

        MessageBrokerLog.ConnectorStarted(Logger, MessageBrokerProvider);

        return Task.CompletedTask;
    }

    public Task StartAsync(RegionEndpoint region)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ThrowIfAlreadyStarted();

        SetClient(() => new AmazonSQSClient(region));

        return Task.CompletedTask;
    }

    public Task StartAsync(AmazonSQSConfig config)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ThrowIfAlreadyStarted();

        SetClient(() => new AmazonSQSClient(config));

        return Task.CompletedTask;
    }

    public Task StartAsync(string accessKey, string clientSecret, RegionEndpoint region)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ThrowIfAlreadyStarted();

        SetClient(() => new AmazonSQSClient(accessKey, clientSecret, region));

        return Task.CompletedTask;
    }

    public Task StartAsync(string accessKey, string clientSecret, AmazonSQSConfig config)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ThrowIfAlreadyStarted();

        SetClient(() => new AmazonSQSClient(accessKey, clientSecret, config));

        return Task.CompletedTask;
    }

    public Task StartAsync(AWSCredentials awsCredentials, RegionEndpoint region)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ThrowIfAlreadyStarted();

        SetClient(() => new AmazonSQSClient(awsCredentials, region));

        return Task.CompletedTask;
    }

    public Task StartAsync(AWSCredentials awsCredentials, AmazonSQSConfig config)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ThrowIfAlreadyStarted();

        SetClient(() => new AmazonSQSClient(awsCredentials, config));

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

        var value = connectionString.Trim();

        Dictionary<string, string> settings;

        try
        {
            settings = JsonDeserializer.Default.Deserialize<Dictionary<string, string>>(value)?
                .ToDictionary(result => result.Key.ToUpperInvariant(), result => result.Value) ??
                    throw new ArgumentException("Connection string is not a valid JSON settings object", nameof(connectionString));
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("Connection string is not a valid JSON settings object", nameof(connectionString), exception);
        }

        var config = new AmazonSQSConfig();

        if (settings.TryGetValue("REGION", out var region) && !string.IsNullOrWhiteSpace(region))
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(region);
        }

        if (settings.TryGetValue("SERVICEURL", out var serviceUrl) && !string.IsNullOrWhiteSpace(serviceUrl))
        {
            config.ServiceURL = serviceUrl;
        }

        var hasAccessKey = settings.TryGetValue("ACCESSKEY", out var accessKey) && !string.IsNullOrWhiteSpace(accessKey);
        var hasSecretKey = settings.TryGetValue("SECRETKEY", out var secretKey) && !string.IsNullOrWhiteSpace(secretKey);

        SetClient(() => hasAccessKey && hasSecretKey ?
            new AmazonSQSClient(new BasicAWSCredentials(accessKey, secretKey), config) :
            new AmazonSQSClient(config));

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
        _queueUrls.Clear();

        _client?.DisposeSafelyIfNeeded();
        _client = null;
    }

    public override Task<ISender> GetSenderAsync(string subject, MessageBrokerAddress? defaultAddress = null, ISerializer? serializer = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        _ = Client;

        var sender = new AmazonSQSSender(subject, new WeakReference<AmazonSQSConnector>(this))
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

        var receiver = new AmazonSQSReceiver(subject, new WeakReference<AmazonSQSConnector>(this))
        {
            Deserializer = deserializer
        };

        _receivers[new WeakReference<AmazonSQSReceiver>(receiver)] = true;

        MessageBrokerLog.ReceiverCreated(Logger, MessageBrokerProvider, subject);

        return Task.FromResult<IReceiver>(receiver);
    }

    public Task<string> DeclareQueueAsync(string queue, IDictionary<string, string>? attributes = null, CancellationToken cancellationToken = default)
        => AmazonSQSCommons.DeclareQueueAsync(() => Client, _queueUrls, Logger, MessageBrokerProvider, queue, attributes, cancellationToken);

    public Task DeleteQueueAsync(string queue, bool ignoreIfDoesNotExist = true, CancellationToken cancellationToken = default)
        => AmazonSQSCommons.DeleteQueueAsync(() => Client, _queueUrls, Logger, MessageBrokerProvider, queue, ignoreIfDoesNotExist, cancellationToken);

    internal async Task<string> GetQueueUrlAsync(MessageBrokerAddress address, CancellationToken cancellationToken)
    {
        var queueName = ExtractQueueName(address);

        if (_queueUrls.TryGetValue(queueName, out var queueUrl))
        {
            return queueUrl;
        }

        var response = await Client.GetQueueUrlAsync(queueName, cancellationToken).ConfigureAwait(false);

        _queueUrls[queueName] = response.QueueUrl;

        return response.QueueUrl;
    }

    internal Task<bool> HasRedrivePolicyAsync(string queueUrl, CancellationToken cancellationToken)
        => AmazonSQSCommons.HasRedrivePolicyAsync(() => Client, queueUrl, cancellationToken);

    internal static string ExtractQueueName(MessageBrokerAddress address)
        => address.Type switch
        {
            MessageBrokerAddressType.Direct => address.Value,
            MessageBrokerAddressType.Queue => address.Value,
            _ => throw new NotSupportedException($"Address type '{address.Type}' is not supported by Amazon SQS; only Queue addresses are valid (Topic/Fanout require SNS)")
        };

    internal static Dictionary<string, MessageAttributeValue> MessageAttributesToSQSAttributes(MessageEnvelope messageEnvelope)
    {
        var sentTimestamp = messageEnvelope.Attributes.TouchSentTimestamp();

        var attributes = new Dictionary<string, MessageAttributeValue>
        {
            [Constants.ContentTypeHeaderKey] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = Constants.ContentTypeValue
            },
            [Constants.ContentEncodingHeaderKey] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = Constants.ContentEncodingValue
            },
            [Constants.MessageIdHeaderKey] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = ValueConversions.GuidToString(messageEnvelope.Attributes.MessageId)
            },
            [Constants.SendTimestampHeaderKey] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = ValueConversions.DateTimeOffsetToString(sentTimestamp)
            }
        };

        if (messageEnvelope.Attributes.PayloadType != null)
        {
            attributes[Constants.PayloadTypeHeaderKey] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = messageEnvelope.Attributes.PayloadType
            };
        }

        if (messageEnvelope.Attributes.TTL.HasValue)
        {
            attributes[Constants.TTLHeaderKey] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = ((long)messageEnvelope.Attributes.TTL.Value.TotalMilliseconds).ToString(CultureInfo.InvariantCulture)
            };
        }

        if (messageEnvelope.Attributes.TraceParent != null)
        {
            attributes[Constants.TraceParentHeaderKey] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = messageEnvelope.Attributes.TraceParent
            };
        }

        if (messageEnvelope.Attributes.TraceState != null)
        {
            attributes[Constants.TraceStateHeaderKey] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = messageEnvelope.Attributes.TraceState
            };
        }

        if (messageEnvelope.Attributes.Baggage != null)
        {
            attributes[Constants.BaggageHeaderKey] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = messageEnvelope.Attributes.Baggage
            };
        }

        return attributes;
    }

    internal static MessageEnvelopeAttributes MessageAttributesFromSQSMessage(Message message)
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
                PayloadType = GetAttributeStringOrDefault(message, Constants.PayloadTypeHeaderKey)
            }
            .FixTimestamps();

            return result;
        }
        catch (Exception exception)
        {
            throw new DeserializeMessageEnvelopeException(exception);
        }
    }

    internal static bool TryGetHeaderString(Message message, string key, out string? value)
    {
        if (!message.MessageAttributes.TryGetValue(key, out var messageValue))
        {
            value = null;

            return false;
        }

        value = messageValue.StringValue;

        return true;
    }

    private static string? GetAttributeStringOrDefault(Message message, string key)
        => (message.MessageAttributes != null) && message.MessageAttributes.TryGetValue(key, out var value) ? value.StringValue : null;
}
