using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using FFCEI.DotNetStack.Serialization.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace FFCEI.DotNetStack.MessageBroker.RabbitMQ;

public sealed class RabbitMQConnector(ILogger<RabbitMQConnector>? logger = null)
        : MessageBrokerConnector(MessageBrokerProviderType.RabbitMQ, logger)
{
    private ConnectionFactory? _connectionFactory;
    private IConnection? _connection;
    private IChannel? _managementChannel;
    private readonly ConcurrentDictionary<IChannel, object> _channels = [];
    private readonly ConcurrentDictionary<WeakReference<RabbitMQReceiver>, object> _receivers = [];

    public IConnection Connection => _connection != null ? _connection! :
        throw new InvalidOperationException("RabbitMQConnector not started");

    public override bool Started()
        => !Disposed && (_connection != null);

    private void ThrowIfAlreadyStarted()
    {
        if (_connection != null)
        {
            throw new InvalidOperationException("RabbitMQConnector already started");
        }
    }

    public async Task StartAsync(ConnectionFactory connectionFactory, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ArgumentNullException.ThrowIfNull(connectionFactory, nameof(connectionFactory));

        ThrowIfAlreadyStarted();

        _connectionFactory = connectionFactory;

        try
        {
            _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
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

    public override Task StartAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri, nameof(uri));
        ArgumentException.ThrowIfNullOrWhiteSpace(uri.Host, nameof(uri.Host));

        return StartAsync(new ConnectionFactory
        {
            Uri = uri,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        },
        cancellationToken);
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

        foreach (var channel in _channels.Keys)
        {
            await ExceptionProtector.ExecuteTaskAsync(channel.CloseAsync()).ConfigureAwait(false);

            await channel.DisposeSafelyIfNeededAsync().ConfigureAwait(false);
        }

        _channels.Clear();

        if (_managementChannel != null)
        {
            await ExceptionProtector.ExecuteTaskAsync(_managementChannel.CloseAsync()).ConfigureAwait(false);

            await _managementChannel.DisposeSafelyIfNeededAsync().ConfigureAwait(false);

            _managementChannel = null;
        }

        if (_connection != null)
        {
            await ExceptionProtector.ExecuteTaskAsync(_connection.CloseAsync()).ConfigureAwait(false);

            await _connection.DisposeSafelyIfNeededAsync().ConfigureAwait(false);

            _connection = null;
        }
    }

    public override async Task<ISender> GetSenderAsync(string subject, MessageBrokerAddress? defaultAddress = null, ISerializer? serializer = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        var channel = await Connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        var sender = new RabbitMQSender(channel, subject, new WeakReference<RabbitMQConnector>(this))
        {
            DefaultAddress = defaultAddress,
            Serializer = serializer
        };

        _channels[channel] = true;

        MessageBrokerLog.SenderCreated(Logger, MessageBrokerProvider, subject);

        return sender;
    }

    public override async Task<IReceiver> GetReceiverAsync(string subject, IDeserializer? deserializer = null, CancellationToken cancellationToken = default)
    {
        var channel = await CreateChannelForReceiverAsync(cancellationToken).ConfigureAwait(false);
        var receiver = new RabbitMQReceiver(channel, subject, new WeakReference<RabbitMQConnector>(this))
        {
            Deserializer = deserializer
        };

        _receivers[new WeakReference<RabbitMQReceiver>(receiver)] = true;

        MessageBrokerLog.ReceiverCreated(Logger, MessageBrokerProvider, subject);

        return receiver;
    }

    internal async Task<IChannel> CreateChannelForReceiverAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        var channel = await Connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        _channels[channel] = true;

        return channel;
    }

    internal async Task CloseChannelAsync(IChannel channel)
    {
        if (_channels.TryRemove(channel, out var _))
        {
            await ExceptionProtector.ExecuteTaskAsync(channel.CloseAsync()).ConfigureAwait(false);
            await ExceptionProtector.ExecuteValueTaskAsync(channel.DisposeAsync()).ConfigureAwait(false);
        }
    }

    private async Task<IChannel> GetOrCreateManagementChannelAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (_managementChannel == null || !_managementChannel.IsOpen)
        {
            await _managementChannel.DisposeSafelyIfNeededAsync().ConfigureAwait(false);

            _managementChannel = null;
        }

        _managementChannel ??= await Connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return _managementChannel;
    }

    public async Task<QueueDeclareOk> DeclareQueueAsync(string queue, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object?>? arguments = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(queue, nameof(queue));

        var channel = await GetOrCreateManagementChannelAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var result = await channel.QueueDeclareAsync(queue, durable, exclusive, autoDelete, arguments, cancellationToken: cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.Declared(Logger, MessageBrokerProvider, "queue", queue);

            return result;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            MessageBrokerLog.DeclareFailed(Logger, exception, MessageBrokerProvider, "queue", queue);

            throw;
        }
    }

    public async Task DeclareExchangeAsync(string exchange, string exchangeType = ExchangeType.Direct, bool durable = true, bool autoDelete = false, IDictionary<string, object?>? arguments = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exchange, nameof(exchange));
        ArgumentException.ThrowIfNullOrWhiteSpace(exchangeType, nameof(exchangeType));

        var channel = await GetOrCreateManagementChannelAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await channel.ExchangeDeclareAsync(exchange, exchangeType, durable, autoDelete, arguments, cancellationToken: cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.Declared(Logger, MessageBrokerProvider, "exchange", exchange);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            MessageBrokerLog.DeclareFailed(Logger, exception, MessageBrokerProvider, "exchange", exchange);

            throw;
        }
    }

    public async Task BindQueueToExchangeAsync(string queue, string exchange, string? routingKey, IDictionary<string, object?>? arguments = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queue, nameof(queue));
        ArgumentException.ThrowIfNullOrWhiteSpace(exchange, nameof(exchange));
        ArgumentNullException.ThrowIfNull(routingKey, nameof(routingKey));

        var channel = await GetOrCreateManagementChannelAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await channel.QueueBindAsync(queue, exchange, routingKey, arguments, cancellationToken: cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.DeclaredBinding(Logger, MessageBrokerProvider, queue, exchange);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            MessageBrokerLog.DeclareBindingFailed(Logger, exception, MessageBrokerProvider, queue, exchange);

            throw;
        }
    }

    public async Task UnbindQueueFromExchangeAsync(string queue, string exchange, string routingKey = "", IDictionary<string, object?>? arguments = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queue, nameof(queue));
        ArgumentException.ThrowIfNullOrWhiteSpace(exchange, nameof(exchange));
        ArgumentNullException.ThrowIfNull(routingKey, nameof(routingKey));

        var channel = await GetOrCreateManagementChannelAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await channel.QueueUnbindAsync(queue, exchange, routingKey, arguments, cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.DeletedBinding(Logger, MessageBrokerProvider, queue, exchange);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            MessageBrokerLog.DeleteBindingFailed(Logger, exception, MessageBrokerProvider, queue, exchange);

            throw;
        }
    }

    public async Task<uint> DeleteQueueAsync(string queue, bool ifUnused = false, bool ifEmpty = false, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queue, nameof(queue));

        var channel = await GetOrCreateManagementChannelAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var purgedMessageCount = await channel.QueueDeleteAsync(queue, ifUnused, ifEmpty, cancellationToken: cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.Deleted(Logger, MessageBrokerProvider, "queue", queue);

            return purgedMessageCount;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            MessageBrokerLog.DeleteFailed(Logger, exception, MessageBrokerProvider, "queue", queue);

            throw;
        }
    }

    public async Task DeleteExchangeAsync(string exchange, bool ifUnused = false, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exchange, nameof(exchange));

        var channel = await GetOrCreateManagementChannelAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await channel.ExchangeDeleteAsync(exchange, ifUnused, cancellationToken: cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.Deleted(Logger, MessageBrokerProvider, "exchange", exchange);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            MessageBrokerLog.DeleteFailed(Logger, exception, MessageBrokerProvider, "exchange", exchange);

            throw;
        }
    }

    internal static BasicProperties MessageAttributesToBasicProperties(MessageEnvelope messageEnvelope)
    {
        var sentTimestamp = messageEnvelope.Attributes.TouchSentTimestamp();

        var headers = new Dictionary<string, object?>()
        {
            [Common.Constants.SendTimestampHeaderKey] = ValueConversions.DateTimeOffsetToString(sentTimestamp)
        };

        if (messageEnvelope.Attributes.TraceParent != null)
        {
            headers[Common.Constants.TraceParentHeaderKey] = messageEnvelope.Attributes.TraceParent;
        }

        if (messageEnvelope.Attributes.TraceState != null)
        {
            headers[Common.Constants.TraceStateHeaderKey] = messageEnvelope.Attributes.TraceState;
        }

        if (messageEnvelope.Attributes.Baggage != null)
        {
            headers[Common.Constants.BaggageHeaderKey] = messageEnvelope.Attributes.Baggage;
        }

        if (messageEnvelope.Attributes.RoutingHeaders != null)
        {
            foreach (var routingHeader in messageEnvelope.Attributes.RoutingHeaders)
            {
                headers[routingHeader.Key] = routingHeader.Value;
            }
        }

        var result = new BasicProperties
        {
            ContentType = Common.Constants.ContentTypeValue,
            ContentEncoding = Common.Constants.ContentEncodingValue,
            Type = messageEnvelope.Attributes.PayloadType,
            MessageId = ValueConversions.GuidToString(messageEnvelope.Attributes.MessageId),
            DeliveryMode = messageEnvelope.Attributes.Durable ? DeliveryModes.Persistent : DeliveryModes.Transient,
            Headers = headers
        };

        if (messageEnvelope.Attributes.TTL.HasValue)
        {
            result.Expiration = ((long)messageEnvelope.Attributes.TTL.Value.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
        }

        return result;
    }

    internal static MessageEnvelopeAttributes MessageAttributesFromBasicProperties(IReadOnlyBasicProperties basicProperties)
    {
        ArgumentNullException.ThrowIfNull(basicProperties, nameof(basicProperties));

        try
        {
            Dictionary<string, object>? mappedHeaders = null;

            if (basicProperties.Headers != null)
            {
                foreach (var header in basicProperties.Headers)
                {
                    if (!Common.Constants.KnownHeaderKeys.Contains(header.Key))
                    {
                        mappedHeaders ??= [];
                        mappedHeaders[header.Key] = header.Value is byte[] headerBytes ? Encoding.UTF8.GetString(headerBytes) : header.Value ?? string.Empty;
                    }
                }
            }

            var routingHeaders = mappedHeaders == null ? MessageEnvelopeAttributes.EmptyRoutingHeaders :
                new ReadOnlyDictionary<string, object>(mappedHeaders);

            var result = new MessageEnvelopeAttributes
            {
                TraceParent = GetHeaderStringOrDefault(basicProperties.Headers, Common.Constants.TraceParentHeaderKey),
                TraceState = GetHeaderStringOrDefault(basicProperties.Headers, Common.Constants.TraceStateHeaderKey),
                Baggage = GetHeaderStringOrDefault(basicProperties.Headers, Common.Constants.BaggageHeaderKey),
                RoutingHeaders = routingHeaders,
                TTL = !string.IsNullOrWhiteSpace(basicProperties.Expiration) ?
                    TimeSpan.FromMilliseconds(long.Parse(basicProperties.Expiration, CultureInfo.InvariantCulture)) : null,
                Durable = basicProperties.DeliveryMode == DeliveryModes.Persistent,
                MessageId = ValueConversions.TryGuidFromString(basicProperties.MessageId) ?? Guid.CreateVersion7(),
                SentTimestamp = ValueConversions.TryDateTimeOffsetFromString(GetHeaderStringOrDefault(basicProperties.Headers, Common.Constants.SendTimestampHeaderKey)),
                PayloadType = basicProperties.Type,
            }
            .FixTimestamps();

            return result;
        }
        catch (Exception exception)
        {
            throw new DeserializeMessageEnvelopeException(exception);
        }
    }

    internal static bool TryGetHeaderString(IDictionary<string, object?>? headers, string key, out string? value)
    {
        value = null;

        if ((headers == null) || !headers.TryGetValue(key, out var header))
        {
            return false;
        }

        value = header is byte[] bytes ? Encoding.UTF8.GetString(bytes) : header?.ToString();

        return true;
    }

    internal static string? GetHeaderStringOrDefault(IDictionary<string, object?>? headers, string key, string? defaultValue = null)
        => TryGetHeaderString(headers, key, out var value) ? value : defaultValue;
}
