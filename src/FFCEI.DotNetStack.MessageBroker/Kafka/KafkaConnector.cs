using Confluent.Kafka;
using Confluent.Kafka.Admin;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using FFCEI.DotNetStack.Serialization;
using FFCEI.DotNetStack.Serialization.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace FFCEI.DotNetStack.MessageBroker.Kafka;

public sealed class KafkaConnector(ILogger<KafkaConnector>? logger = null)
        : MessageBrokerConnector(MessageBrokerProviderType.Kafka, logger)
{
    private readonly ConcurrentDictionary<IProducer<Null, byte[]>, object> _producers = [];
    private readonly ConcurrentDictionary<IConsumer<Ignore, byte[]>, object> _consumers = [];
    private readonly ConcurrentDictionary<WeakReference<KafkaReceiver>, object> _receivers = [];

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ClientConfig? ClientConfig { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Dictionary<string, string>? Settings { get; set; }

    private static IProducer<Null, byte[]> CreateKafkaProducer(ProducerConfig producerConfig)
        => new ProducerBuilder<Null, byte[]>(producerConfig).Build();

    private static IConsumer<Ignore, byte[]> CreateKafkaConsumer(ConsumerConfig consumerConfig)
        => new ConsumerBuilder<Ignore, byte[]>(consumerConfig).Build();

    public override bool Started()
        => !Disposed && (ClientConfig != null || Settings != null);

    private void ThrowIfAlreadyStarted()
    {
        if (ClientConfig != null || Settings != null)
        {
            throw new InvalidOperationException("KafkaConnector already started");
        }
    }

    public Task StartAsync(ClientConfig clientConfig)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ArgumentNullException.ThrowIfNull(clientConfig, nameof(clientConfig));

        ThrowIfAlreadyStarted();

        ClientConfig = clientConfig;

        MessageBrokerLog.ConnectorStarted(Logger, MessageBrokerProvider);

        return Task.CompletedTask;
    }

    public Task StartAsync(Dictionary<string, string> settings)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ArgumentNullException.ThrowIfNull(settings, nameof(settings));

        ThrowIfAlreadyStarted();

        Settings = settings;

        MessageBrokerLog.ConnectorStarted(Logger, MessageBrokerProvider);

        return Task.CompletedTask;
    }

    public override Task StartAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ArgumentNullException.ThrowIfNull(uri, nameof(uri));
        ArgumentException.ThrowIfNullOrWhiteSpace(uri.Host, nameof(uri.Host));

        ThrowIfAlreadyStarted();

        var settings = new Dictionary<string, string>
        {
            ["bootstrap.servers"] = $"{uri.Host}:{(uri.IsDefaultPort ? 9092 : uri.Port)}"
        };

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            var separatorIndex = uri.UserInfo.IndexOf(':', StringComparison.Ordinal);

            settings["security.protocol"] = "SASL_PLAINTEXT";
            settings["sasl.mechanism"] = "PLAIN";
            settings["sasl.username"] = Uri.UnescapeDataString(separatorIndex < 0 ? uri.UserInfo : uri.UserInfo[..separatorIndex]);

            if (separatorIndex >= 0)
            {
                settings["sasl.password"] = Uri.UnescapeDataString(uri.UserInfo[(separatorIndex + 1)..]);
            }
        }

        return StartAsync(settings);
    }

    public override Task StartAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

        ThrowIfAlreadyStarted();

        Dictionary<string, string> settings;

        try
        {
            settings = JsonDeserializer.Default.Deserialize<Dictionary<string, string>>(connectionString) ??
                throw new ArgumentException("Connection string is not a valid JSON settings object", nameof(connectionString));
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("Connection string is not a valid JSON settings object", nameof(connectionString), exception);
        }

        return StartAsync(settings);
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

        foreach (var producer in _producers.Keys)
        {
            ExceptionProtector.ExecuteAction(() => producer.Flush(TimeSpan.FromSeconds(10)));

            producer.DisposeSafelyIfNeeded();
        }

        _producers.Clear();

        foreach (var consumer in _consumers.Keys)
        {
            ExceptionProtector.ExecuteAction(() => consumer.Close());

            consumer.DisposeSafelyIfNeeded();
        }

        _consumers.Clear();

        ClientConfig = null;
        Settings = null;
    }

    public override Task<ISender> GetSenderAsync(string subject, MessageBrokerAddress? defaultAddress = null, ISerializer? serializer = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (ClientConfig == null && Settings == null)
        {
            throw new InvalidOperationException("KafkaConnector not started");
        }

        var clientConfig = GetKafkaProducerClientConfig() ??
            throw new InvalidOperationException("KafkaConnector not started");

        var producerConfig = new ProducerConfig(clientConfig);

        producerConfig.EnableIdempotence ??= true;

#pragma warning disable CA2000 // Dispose objects before losing scope
        var producer = CreateKafkaProducer(producerConfig);
#pragma warning restore CA2000 // Dispose objects before losing scope

        var sender = new KafkaSender(producer, subject, new WeakReference<KafkaConnector>(this))
        {
            DefaultAddress = defaultAddress,
            Serializer = serializer
        };

        _producers[producer] = true;

        MessageBrokerLog.SenderCreated(Logger, MessageBrokerProvider, subject);

        return Task.FromResult<ISender>(sender);
    }

    internal async Task CloseProducerAsync(IProducer<Null, byte[]> producer)
    {
        if (_producers.TryRemove(producer, out var _))
        {
            ExceptionProtector.ExecuteAction(() => producer.Flush(TimeSpan.FromSeconds(10)));

            producer.DisposeSafelyIfNeeded();
        }
    }

    public override Task<IReceiver> GetReceiverAsync(string subject, IDeserializer? deserializer = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

#pragma warning disable CA2000 // Dispose objects before losing scope
        var consumer = CreateConsumerForReceiver(subject);
#pragma warning restore CA2000 // Dispose objects before losing scope

        var receiver = new KafkaReceiver(consumer, subject, new WeakReference<KafkaConnector>(this))
        {
            Deserializer = deserializer
        };

        _receivers[new WeakReference<KafkaReceiver>(receiver)] = true;

        MessageBrokerLog.ReceiverCreated(Logger, MessageBrokerProvider, subject);

        return Task.FromResult<IReceiver>(receiver);
    }

    internal IConsumer<Ignore, byte[]> CreateConsumerForReceiver(string subject)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (ClientConfig == null && Settings == null)
        {
            throw new InvalidOperationException("KafkaConnector not started");
        }

        var clientConfig = GetKafkaConsumerClientConfig() ??
            throw new InvalidOperationException("KafkaConnector not started");

        var consumerConfig = new ConsumerConfig(clientConfig)
        {
            GroupId = string.IsNullOrWhiteSpace(subject) ? "default" : subject,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

#pragma warning disable CA2000 // Dispose objects before losing scope
        var consumer = CreateKafkaConsumer(consumerConfig);
#pragma warning restore CA2000 // Dispose objects before losing scope

        _consumers[consumer] = true;

        return consumer;
    }

    internal async Task CloseConsumerAsync(IConsumer<Ignore, byte[]> consumer)
    {
        if (_consumers.TryRemove(consumer, out var _))
        {
            ExceptionProtector.ExecuteAction(() => consumer.Close());

            consumer.DisposeSafelyIfNeeded();
        }
    }

    public Task CreateTopicAsync(TopicSpecification topicSpecification, bool ignoreIfTopicAlreadyExists = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(topicSpecification, nameof(topicSpecification));

        return CreateTopicsAsync([topicSpecification], ignoreIfTopicAlreadyExists, cancellationToken);
    }

    public async Task CreateTopicsAsync(IReadOnlyCollection<TopicSpecification> topicSpecifications, bool ignoreIfTopicAlreadyExists = true, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ArgumentNullException.ThrowIfNull(topicSpecifications, nameof(topicSpecifications));

        if (topicSpecifications.Count == 0)
        {
            return;
        }

        var clientConfig = GetKafkaProducerClientConfig() ??
            throw new InvalidOperationException("KafkaConnector not started");

        var adminClientConfig = new AdminClientConfig(clientConfig);

        using var adminClient = new AdminClientBuilder(adminClientConfig).Build();

        try
        {
            await adminClient.CreateTopicsAsync(topicSpecifications).WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (CreateTopicsException exception) when (ignoreIfTopicAlreadyExists &&
            exception.Results.TrueForAll(result => !result.Error.IsError || (result.Error.Code == ErrorCode.TopicAlreadyExists)))
        {
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            foreach (var topicSpecification in topicSpecifications)
            {
                MessageBrokerLog.DeclareFailed(Logger, exception, MessageBrokerProvider, "topic", topicSpecification.Name);
            }

            throw;
        }

        foreach (var topicSpecification in topicSpecifications)
        {
            MessageBrokerLog.Declared(Logger, MessageBrokerProvider, "topic", topicSpecification.Name);
        }
    }

    public Task DeleteTopicAsync(string topic, bool ignoreIfTopicDoesNotExist = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(topic, nameof(topic));

        return DeleteTopicsAsync([topic], ignoreIfTopicDoesNotExist, cancellationToken);
    }

    public async Task DeleteTopicsAsync(IReadOnlyCollection<string> topics, bool ignoreIfTopicDoesNotExist = true, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ArgumentNullException.ThrowIfNull(topics, nameof(topics));
        ArgumentNullExceptionExtensionMethods.ThrowArgumentNullExceptionIfAny(topics, nameof(topics));

        if (topics.Count == 0)
        {
            return;
        }

        var clientConfig = GetKafkaProducerClientConfig() ??
            throw new InvalidOperationException("KafkaConnector not started");

        var adminClientConfig = new AdminClientConfig(clientConfig);

        using var adminClient = new AdminClientBuilder(adminClientConfig).Build();

        try
        {
            await adminClient.DeleteTopicsAsync(topics).WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DeleteTopicsException exception) when (ignoreIfTopicDoesNotExist &&
            exception.Results.TrueForAll(result => !result.Error.IsError || (result.Error.Code == ErrorCode.UnknownTopicOrPart)))
        {
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            foreach (var topic in topics)
            {
                MessageBrokerLog.DeleteFailed(Logger, exception, MessageBrokerProvider, "topic", topic);
            }

            throw;
        }

        foreach (var topic in topics)
        {
            MessageBrokerLog.Deleted(Logger, MessageBrokerProvider, "topic", topic);
        }
    }

    private static void ApplyCommonClientConfigProperties(Dictionary<string, string>? result)
    {
        if (result != null)
        {
            if (!result.ContainsKey("socket.keepalive.enable"))
            {
                result["socket.keepalive.enable"] = "true";
            }

            if (!result.ContainsKey("reconnect.backoff.max.ms"))
            {
                result["reconnect.backoff.max.ms"] = "500";
            }
        }
    }

    private static void ApplyConsumerClientConfigProperties(Dictionary<string, string>? result)
    {
        if (result != null)
        {
            if (!result.ContainsKey("session.timeout.ms"))
            {
                result["session.timeout.ms"] = "10000";
            }

            if (!result.ContainsKey("max.poll.interval.ms"))
            {
                result["max.poll.interval.ms"] = "300000";
            }

            if (!result.ContainsKey("auto.offset.reset"))
            {
                result["auto.offset.reset"] = "earliest";
            }
        }
    }

    private Dictionary<string, string>? GetKafkaProducerClientConfig()
    {
        var result = ClientConfig != null ? new Dictionary<string, string>(ClientConfig) :
                Settings != null ? new Dictionary<string, string>(Settings) : null;

        ApplyCommonClientConfigProperties(result);

        return result;
    }

    private Dictionary<string, string>? GetKafkaConsumerClientConfig()
    {
        var result = ClientConfig != null ? new Dictionary<string, string>(ClientConfig) :
                Settings != null ? new Dictionary<string, string>(Settings) : null;

        ApplyCommonClientConfigProperties(result);
        ApplyConsumerClientConfigProperties(result);

        return result;
    }

    internal static string ExtractKakfaTopicFromAddress(MessageBrokerAddress address)
        => address.Type switch
        {
            MessageBrokerAddressType.Direct => address.Value,
            MessageBrokerAddressType.Topic => address.Value,
            MessageBrokerAddressType.Queue => address.Value,
            _ => string.IsNullOrWhiteSpace(address.CustomExchange) ? throw new NotSupportedException($"Address type '{address.Type}' is not supported") :
                throw new NotSupportedException($"Custom address type '{address.CustomExchange}' is not supported by Kafka; only Topic addresses are valid.")
        };

    internal static Headers MessageAttributesToKafkaHeaders(MessageEnvelope messageEnvelope)
    {
        var sentTimestamp = messageEnvelope.Attributes.TouchSentTimestamp();

        var headers = new Headers
        {
            { Constants.ContentTypeHeaderKey, Encoding.UTF8.GetBytes(
                Constants.ContentTypeValue) },

            { Constants.ContentEncodingHeaderKey, Encoding.UTF8.GetBytes(
                Constants.ContentEncodingValue) },

            { Constants.MessageIdHeaderKey, Encoding.UTF8.GetBytes(
                ValueConversions.GuidToString(messageEnvelope.Attributes.MessageId)) },

            { Constants.SendTimestampHeaderKey, Encoding.UTF8.GetBytes(
                ValueConversions.DateTimeOffsetToString(sentTimestamp)) }
        };

        if (messageEnvelope.Attributes.PayloadType != null)
        {
            headers.Add(Constants.PayloadTypeHeaderKey, Encoding.UTF8.GetBytes(
                messageEnvelope.Attributes.PayloadType));
        }

        if (messageEnvelope.Attributes.TTL.HasValue)
        {
            headers.Add(Constants.TTLHeaderKey, Encoding.UTF8.GetBytes(
                ((long)messageEnvelope.Attributes.TTL.Value.TotalMilliseconds).ToString(CultureInfo.InvariantCulture)));
        }

        if (messageEnvelope.Attributes.TraceParent != null)
        {
            headers.Add(Constants.TraceParentHeaderKey, Encoding.UTF8.GetBytes(
                messageEnvelope.Attributes.TraceParent));
        }

        if (messageEnvelope.Attributes.TraceState != null)
        {
            headers.Add(Constants.TraceStateHeaderKey, Encoding.UTF8.GetBytes(
                messageEnvelope.Attributes.TraceState));
        }

        if (messageEnvelope.Attributes.Baggage != null)
        {
            headers.Add(Constants.BaggageHeaderKey, Encoding.UTF8.GetBytes(
                messageEnvelope.Attributes.Baggage));
        }

        return headers;
    }

    internal static MessageEnvelopeAttributes MessageAttributesFromConsumeResult(ConsumeResult<Ignore, byte[]> consumeResult)
    {
        try
        {
            var headers = consumeResult.Message.Headers;

            var result = new MessageEnvelopeAttributes
            {
                TraceParent = GetHeaderStringOrDefault(headers, Constants.TraceParentHeaderKey),
                TraceState = GetHeaderStringOrDefault(headers, Constants.TraceStateHeaderKey),
                Baggage = GetHeaderStringOrDefault(headers, Constants.BaggageHeaderKey),
                RoutingHeaders = MessageEnvelopeAttributes.EmptyRoutingHeaders,
                TTL = TryGetHeaderString(headers, Constants.TTLHeaderKey, out var ttlText) &&
                    !string.IsNullOrWhiteSpace(ttlText) ? TimeSpan.FromMilliseconds(long.Parse(ttlText, CultureInfo.InvariantCulture)) : null,
                Durable = false,
                MessageId = ValueConversions.TryGuidFromString(GetHeaderStringOrDefault(headers, Constants.MessageIdHeaderKey)) ?? Guid.CreateVersion7(),
                SentTimestamp = ValueConversions.TryDateTimeOffsetFromString(GetHeaderStringOrDefault(headers, Constants.SendTimestampHeaderKey)),
                PayloadType = GetHeaderStringOrDefault(headers, Constants.PayloadTypeHeaderKey),
            }
            .FixTimestamps();

            return result;
        }
        catch (Exception exception)
        {
            throw new DeserializeMessageEnvelopeException(exception);
        }
    }

    internal static bool TryGetHeaderString(Headers? headers, string key, out string? value)
    {
        value = null;

        if ((headers == null) || !headers.TryGetLastBytes(key, out var bytes))
        {
            return false;
        }

        value = Encoding.UTF8.GetString(bytes);

        return true;
    }

    internal static string? GetHeaderStringOrDefault(Headers? headers, string key, string? defaultValue = null)
        => TryGetHeaderString(headers, key, out var value) ? value : defaultValue;
}
