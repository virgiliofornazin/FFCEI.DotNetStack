using Apache.Qpid.Proton.Client;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;
using FFCEI.DotNetStack.Serialization.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Text;

namespace FFCEI.DotNetStack.MessageBroker.Qpid;

public sealed class QpidConnector(ILogger<QpidConnector>? logger = null)
        : MessageBrokerConnector(MessageBrokerProviderType.Qpid, logger)
{
#pragma warning disable CA2213 // Disposable fields should be disposed
    private IClient? _client;
    private IConnection? _connection;
#pragma warning restore CA2213 // Disposable fields should be disposed
    private readonly ConcurrentDictionary<ISender, object> _senderLinks = [];
    private readonly ConcurrentDictionary<IReceiver, object> _receiverLinks = [];
    private readonly ConcurrentDictionary<WeakReference<QpidReceiver>, object> _receivers = [];

    public IConnection Connection => _connection != null ? _connection! :
        throw new InvalidOperationException("QpidConnector not started");

    public override bool Started()
        => !Disposed && (_connection != null);

    private void ThrowIfAlreadyStarted()
    {
        if (_connection != null)
        {
            throw new InvalidOperationException("QpidConnector already started");
        }
    }

    public async Task StartAsync(string host, int port, ConnectionOptions connectionOptions, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ArgumentException.ThrowIfNullOrWhiteSpace(host, nameof(host));
        ArgumentNullException.ThrowIfNull(connectionOptions, nameof(connectionOptions));

        ThrowIfAlreadyStarted();

        connectionOptions.ReconnectEnabled = true;

        _client = IClient.Create(new ClientOptions());

        try
        {
            var connection = _client.Connect(host, port, connectionOptions);

            await Task.Yield();

            _ = await connection.OpenTask.WaitAsync(cancellationToken).ConfigureAwait(false);

            await Task.Yield();

            _connection = connection;
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
        ObjectDisposedException.ThrowIf(Disposed, this);

        ArgumentNullException.ThrowIfNull(uri, nameof(uri));
        ArgumentException.ThrowIfNullOrWhiteSpace(uri.Host, nameof(uri.Host));

        ThrowIfAlreadyStarted();

        var sslEnabled = string.Equals(uri.Scheme, "amqps", StringComparison.OrdinalIgnoreCase);

        var connectionOptions = new ConnectionOptions
        {
            SslEnabled = sslEnabled,
            ReconnectEnabled = true
        };

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            var separatorIndex = uri.UserInfo.IndexOf(':', StringComparison.Ordinal);

            connectionOptions.User = Uri.UnescapeDataString(separatorIndex < 0 ? uri.UserInfo : uri.UserInfo[..separatorIndex]);
            connectionOptions.Password = separatorIndex < 0 ? null : Uri.UnescapeDataString(uri.UserInfo[(separatorIndex + 1)..]);
        }

        var port = uri.IsDefaultPort ? (sslEnabled ? 5671 : 5672) : uri.Port;

        return StartAsync(uri.Host, port, connectionOptions, cancellationToken);
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
        }

        _receiverLinks.Clear();

        foreach (var senderLink in _senderLinks.Keys)
        {
            await ExceptionProtector.ExecuteTaskAsync(senderLink.CloseAsync()).ConfigureAwait(false);
        }

        _senderLinks.Clear();

        if (_connection != null)
        {
            await ExceptionProtector.ExecuteTaskAsync(_connection.CloseAsync()).ConfigureAwait(false);

            _connection = null;
        }

        if (_client != null)
        {
            await ExceptionProtector.ExecuteTaskAsync(_client.CloseAsync()).ConfigureAwait(false);

            _client = null;
        }
    }

    public override Task<Interfaces.ISender> GetSenderAsync(string subject, MessageBrokerAddress? defaultAddress = null, ISerializer? serializer = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        _ = Connection;

        var sender = new QpidSender(subject, new WeakReference<QpidConnector>(this))
        {
            DefaultAddress = defaultAddress,
            Serializer = serializer
        };

        MessageBrokerLog.SenderCreated(Logger, MessageBrokerProvider, subject);

        return Task.FromResult<Interfaces.ISender>(sender);
    }

    public override Task<Interfaces.IReceiver> GetReceiverAsync(string subject, IDeserializer? deserializer = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        _ = Connection;

        var receiver = new QpidReceiver(subject, new WeakReference<QpidConnector>(this))
        {
            Deserializer = deserializer
        };

        _receivers[new WeakReference<QpidReceiver>(receiver)] = true;

        MessageBrokerLog.ReceiverCreated(Logger, MessageBrokerProvider, subject);

        return Task.FromResult<Interfaces.IReceiver>(receiver);
    }

    protected override void OnDispose()
    {
        _client.DisposeObjectSafelyIfNeeded();
        _connection.DisposeObjectSafelyIfNeeded();
    }

    protected override async ValueTask OnDisposeAsync()
    {
        await _client.DisposeObjectSafelyIfNeededAsync().ConfigureAwait(false);
        await _connection.DisposeObjectSafelyIfNeededAsync().ConfigureAwait(false);
    }

    internal async Task<ISender> OpenSenderLinkAsync(string address, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        var senderLink = await Connection.OpenSenderAsync(address, new SenderOptions()).WaitAsync(cancellationToken).ConfigureAwait(false);

        await Task.Yield();

        _ = await senderLink.OpenTask.WaitAsync(cancellationToken).ConfigureAwait(false);

        await Task.Yield();

        _senderLinks[senderLink] = true;

        return senderLink;
    }

    internal async Task CloseSenderLinkAsync(ISender senderLink)
    {
        if (_senderLinks.TryRemove(senderLink, out var _))
        {
            await ExceptionProtector.ExecuteTaskAsync(senderLink.CloseAsync()).ConfigureAwait(false);
        }
    }

    internal async Task<IReceiver> OpenReceiverLinkAsync(string address, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        var receiverOptions = new ReceiverOptions
        {
            AutoAccept = false,
            AutoSettle = false
        };

        var receiverLink = await Connection.OpenReceiverAsync(address, receiverOptions).WaitAsync(cancellationToken).ConfigureAwait(false);

        await Task.Yield();

        _ = await receiverLink.OpenTask.WaitAsync(cancellationToken).ConfigureAwait(false);

        await Task.Yield();

        _receiverLinks[receiverLink] = true;

        return receiverLink;
    }

    internal async Task CloseReceiverLinkAsync(IReceiver receiverLink)
    {
        if (_receiverLinks.TryRemove(receiverLink, out var _))
        {
            await ExceptionProtector.ExecuteTaskAsync(receiverLink.CloseAsync()).ConfigureAwait(false);
        }
    }

    private const string QmfManagementAddress = "qmf.default.direct";
    private const string QmfBrokerObjectName = "org.apache.qpid.broker:broker:amqp-broker";

    private static readonly TimeSpan _qmfManagementRequestTimeout = TimeSpan.FromSeconds(30);

    private Task QmfManagementRequestAsync(string methodName, Dictionary<string, object?> methodArguments, CancellationToken cancellationToken)
        => QmfManagementRequestAsync(QmfBrokerObjectName, methodName, methodArguments, cancellationToken);

    private static string QmfExchangeObjectName(string exchange)
        => $"org.apache.qpid.broker:exchange:{exchange}";

    private async Task QmfManagementRequestAsync(string objectName, string methodName, Dictionary<string, object?> methodArguments, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        IReceiver replyReceiver = null!;

        try
        {
            replyReceiver = await Connection.OpenDynamicReceiverAsync(new ReceiverOptions(), null).WaitAsync(cancellationToken).ConfigureAwait(false);

            await Task.Yield();

            _ = await replyReceiver.OpenTask.WaitAsync(_qmfManagementRequestTimeout, cancellationToken).ConfigureAwait(false);

            await Task.Yield();

            var senderLink = await Connection.OpenSenderAsync(QmfManagementAddress, new SenderOptions()).WaitAsync(cancellationToken).ConfigureAwait(false);

            await Task.Yield();

            _ = await senderLink.OpenTask.WaitAsync(_qmfManagementRequestTimeout, cancellationToken).ConfigureAwait(false);

            await Task.Yield();

            try
            {
                var request = IMessage<object>.Create();

                request.Body = new Dictionary<string, object?>
                {
                    ["_object_id"] = new Dictionary<string, object?>
                    {
                        ["_object_name"] = objectName
                    },
                    ["_method_name"] = methodName,
                    ["_arguments"] = methodArguments
                };

                request.Subject = "broker";
                request.ReplyTo = replyReceiver.Source?.Address ?? replyReceiver.Address;
                request.CorrelationId = ValueConversions.GuidToString(Guid.CreateVersion7());

                _ = request.SetProperty("x-amqp-0-10.app-id", "qmf2");
                _ = request.SetProperty("qmf.opcode", "_method_request");

                _ = await senderLink.SendAsync(request).WaitAsync(cancellationToken).ConfigureAwait(false);

                var delivery = await replyReceiver.ReceiveAsync(_qmfManagementRequestTimeout).WaitAsync(cancellationToken).ConfigureAwait(false) ??
                    throw new QpidManagementException($"QMF management request '{methodName}' timed out");

                var response = delivery.Message();

                await ExceptionProtector.ExecuteTaskAsync(delivery.AcceptAsync()).ConfigureAwait(false);

                if (string.Equals(response.GetProperty("qmf.opcode")?.ToString(), "_exception", StringComparison.Ordinal))
                {
                    throw new QpidManagementException(ExtractQmfErrorText(response.Body) ?? $"QMF management request '{methodName}' failed");
                }
            }
            finally
            {
                await ExceptionProtector.ExecuteTaskAsync(senderLink.CloseAsync()).ConfigureAwait(false);
            }
        }
        finally
        {
            if (replyReceiver != null)
            {
                await ExceptionProtector.ExecuteTaskAsync(replyReceiver.CloseAsync()).ConfigureAwait(false);
            }
        }
    }

    private static object? FindQmfMapValue(System.Collections.IDictionary map, string key)
    {
        foreach (System.Collections.DictionaryEntry entry in map)
        {
            if (string.Equals(entry.Key?.ToString(), key, StringComparison.Ordinal))
            {
                return entry.Value;
            }
        }

        return null;
    }

    private static string? ExtractQmfErrorText(object? responseBody)
    {
        if (responseBody is not System.Collections.IDictionary responseMap)
        {
            return null;
        }

        if (FindQmfMapValue(responseMap, "_values") is not System.Collections.IDictionary valuesMap)
        {
            return null;
        }

        var errorText = FindQmfMapValue(valuesMap, "error_text");

        if (errorText is byte[] errorTextBytes)
        {
            return Encoding.UTF8.GetString(errorTextBytes);
        }

        if (errorText is Apache.Qpid.Proton.Buffer.IProtonBuffer errorTextBuffer)
        {
            return errorTextBuffer.ToString(Encoding.UTF8);
        }

        return errorText?.ToString();
    }

    private static bool IsQmfAlreadyExistsError(QpidManagementException exception)
        => exception.Message.Contains("already exist", StringComparison.OrdinalIgnoreCase);

    private static bool IsQmfDoesNotExistError(QpidManagementException exception)
        => exception.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase) ||
            exception.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
            exception.Message.Contains("no such", StringComparison.OrdinalIgnoreCase);

    private static Dictionary<string, object?> BuildQmfProperties(Dictionary<string, object?> properties, IDictionary<string, object?>? arguments)
    {
        if (arguments != null)
        {
            foreach (var argument in arguments)
            {
                properties[argument.Key] = argument.Value;
            }
        }

        return properties;
    }

    public async Task DeclareQueueAsync(string queue, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object?>? arguments = null, bool ignoreIfAlreadyExists = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queue, nameof(queue));

        var properties = BuildQmfProperties(new Dictionary<string, object?>
        {
            ["durable"] = durable,
            ["exclusive"] = exclusive,
            ["auto-delete"] = autoDelete
        }, arguments);

        try
        {
            await QmfManagementRequestAsync("create", new Dictionary<string, object?>
            {
                ["type"] = "queue",
                ["name"] = queue,
                ["properties"] = properties,
                ["strict"] = true
            }, cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.Declared(Logger, MessageBrokerProvider, "queue", queue);
        }
        catch (QpidManagementException exception) when (ignoreIfAlreadyExists && IsQmfAlreadyExistsError(exception))
        {
        }
        catch (QpidManagementException exception)
        {
            MessageBrokerLog.DeclareFailed(Logger, exception, MessageBrokerProvider, "queue", queue);

            throw;
        }
    }

    public async Task DeclareExchangeAsync(string exchange, string exchangeType = "direct", bool durable = true, bool autoDelete = false, IDictionary<string, object?>? arguments = null, bool ignoreIfAlreadyExists = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exchange, nameof(exchange));
        ArgumentException.ThrowIfNullOrWhiteSpace(exchangeType, nameof(exchangeType));

        var properties = BuildQmfProperties(new Dictionary<string, object?>
        {
            ["exchange-type"] = exchangeType,
            ["durable"] = durable,
            ["auto-delete"] = autoDelete
        }, arguments);

        try
        {
            await QmfManagementRequestAsync("create", new Dictionary<string, object?>
            {
                ["type"] = "exchange",
                ["name"] = exchange,
                ["properties"] = properties,
                ["strict"] = true
            }, cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.Declared(Logger, MessageBrokerProvider, "exchange", exchange);
        }
        catch (QpidManagementException exception) when (ignoreIfAlreadyExists && IsQmfAlreadyExistsError(exception))
        {
        }
        catch (QpidManagementException exception)
        {
            MessageBrokerLog.DeclareFailed(Logger, exception, MessageBrokerProvider, "exchange", exchange);

            throw;
        }
    }

    public async Task BindQueueToExchangeAsync(string queue, string exchange, string bindingKey = "", IDictionary<string, object?>? arguments = null, bool ignoreIfAlreadyExists = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queue, nameof(queue));
        ArgumentException.ThrowIfNullOrWhiteSpace(exchange, nameof(exchange));
        ArgumentNullException.ThrowIfNull(bindingKey, nameof(bindingKey));

        try
        {
            await QmfManagementRequestAsync("create", new Dictionary<string, object?>
            {
                ["type"] = "binding",
                ["name"] = $"{exchange}/{queue}/{bindingKey}",
                ["properties"] = BuildQmfProperties([], arguments),
                ["strict"] = true
            }, cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.DeclaredBinding(Logger, MessageBrokerProvider, queue, exchange);
        }
        catch (QpidManagementException exception) when (ignoreIfAlreadyExists && IsQmfAlreadyExistsError(exception))
        {
        }
        catch (QpidManagementException exception)
        {
            MessageBrokerLog.DeclareBindingFailed(Logger, exception, MessageBrokerProvider, queue, exchange);

            throw;
        }
    }

    public async Task UnbindQueueFromExchangeAsync(string queue, string exchange, string bindingKey = "", bool ignoreIfDoesNotExist = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queue, nameof(queue));
        ArgumentException.ThrowIfNullOrWhiteSpace(exchange, nameof(exchange));
        ArgumentNullException.ThrowIfNull(bindingKey, nameof(bindingKey));

        try
        {
            await QmfManagementRequestAsync(QmfExchangeObjectName(exchange), "unbind", new Dictionary<string, object?>
            {
                ["queue"] = queue,
                ["bindingKey"] = bindingKey
            }, cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.DeletedBinding(Logger, MessageBrokerProvider, queue, exchange);
        }
        catch (QpidManagementException exception) when (ignoreIfDoesNotExist && IsQmfDoesNotExistError(exception))
        {
        }
        catch (QpidManagementException exception)
        {
            MessageBrokerLog.DeleteBindingFailed(Logger, exception, MessageBrokerProvider, queue, exchange);

            throw;
        }
    }

    public async Task DeleteQueueAsync(string queue, bool ignoreIfDoesNotExist = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queue, nameof(queue));

        try
        {
            await QmfManagementRequestAsync("delete", new Dictionary<string, object?>
            {
                ["type"] = "queue",
                ["name"] = queue,
                ["options"] = new Dictionary<string, object?>()
            }, cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.Deleted(Logger, MessageBrokerProvider, "queue", queue);
        }
        catch (QpidManagementException exception) when (ignoreIfDoesNotExist && IsQmfDoesNotExistError(exception))
        {
        }
        catch (QpidManagementException exception)
        {
            MessageBrokerLog.DeleteFailed(Logger, exception, MessageBrokerProvider, "queue", queue);

            throw;
        }
    }

    public async Task DeleteExchangeAsync(string exchange, bool ignoreIfDoesNotExist = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exchange, nameof(exchange));

        try
        {
            await QmfManagementRequestAsync("delete", new Dictionary<string, object?>
            {
                ["type"] = "exchange",
                ["name"] = exchange,
                ["options"] = new Dictionary<string, object?>()
            }, cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.Deleted(Logger, MessageBrokerProvider, "exchange", exchange);
        }
        catch (QpidManagementException exception) when (ignoreIfDoesNotExist && IsQmfDoesNotExistError(exception))
        {
        }
        catch (QpidManagementException exception)
        {
            MessageBrokerLog.DeleteFailed(Logger, exception, MessageBrokerProvider, "exchange", exchange);

            throw;
        }
    }

    internal static string ExtractQpidAddress(MessageBrokerAddress address)
    {
        AmqpCommons.ExtractExchangeAndRoutingKey(address, out var exchange, out var routingKey);

        if (string.IsNullOrWhiteSpace(exchange))
        {
            return routingKey;
        }

        return $"{exchange}/{routingKey}";
    }

    internal static IMessage<byte[]> MessageAttributesToQpidProtonMessage(MessageEnvelope messageEnvelope)
    {
        var sentTimestamp = messageEnvelope.Attributes.TouchSentTimestamp();

        var message = IMessage<byte[]>.Create();

        message.ContentType = Constants.ContentTypeValue;
        message.ContentEncoding = Constants.ContentEncodingValue;
        _ = message.SetProperty(Constants.PayloadTypeHeaderKey, messageEnvelope.Attributes.PayloadType);
        message.MessageId = ValueConversions.GuidToString(messageEnvelope.Attributes.MessageId);
        message.Durable = messageEnvelope.Attributes.Durable;
        _ = message.SetProperty(Constants.SendTimestampHeaderKey, ValueConversions.DateTimeOffsetToString(sentTimestamp));

        if (messageEnvelope.Attributes.TraceParent != null)
        {
            _ = message.SetProperty(Constants.TraceParentHeaderKey, messageEnvelope.Attributes.TraceParent);
        }

        if (messageEnvelope.Attributes.TraceState != null)
        {
            _ = message.SetProperty(Constants.TraceStateHeaderKey, messageEnvelope.Attributes.TraceState);
        }

        if (messageEnvelope.Attributes.Baggage != null)
        {
            _ = message.SetProperty(Constants.BaggageHeaderKey, messageEnvelope.Attributes.Baggage);
        }

        if (messageEnvelope.Attributes.RoutingHeaders != null)
        {
            foreach (var routingHeader in messageEnvelope.Attributes.RoutingHeaders)
            {
                _ = message.SetProperty(routingHeader.Key, routingHeader.Value);
            }
        }

        if (messageEnvelope.Attributes.TTL.HasValue)
        {
            var ttlMilliseconds = messageEnvelope.Attributes.TTL.Value.TotalMilliseconds;

            message.TimeToLive = ttlMilliseconds >= uint.MaxValue ? uint.MaxValue : (uint)ttlMilliseconds;
        }

        if (messageEnvelope.PayloadBytes != null)
        {
            message.Body = messageEnvelope.PayloadBytes;
        }

        return message;
    }

    internal static MessageEnvelopeAttributes MessageAttributesFromQpidProtonMessage(IMessage<object> message)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        try
        {
            Dictionary<string, object>? mappedHeaders = null;

            _ = message.ForEachProperty((key, value) =>
            {
                if (!Constants.KnownHeaderKeys.Contains(key))
                {
                    mappedHeaders ??= [];
                    mappedHeaders[key] = value;
                }
            });

            var routingHeaders = mappedHeaders == null ? MessageEnvelopeAttributes.EmptyRoutingHeaders :
                new ReadOnlyDictionary<string, object>(mappedHeaders);

            var result = new MessageEnvelopeAttributes
            {
                TraceParent = message.GetProperty(Constants.TraceParentHeaderKey)?.ToString(),
                TraceState = message.GetProperty(Constants.TraceStateHeaderKey)?.ToString(),
                Baggage = message.GetProperty(Constants.BaggageHeaderKey)?.ToString(),
                RoutingHeaders = routingHeaders,
                TTL = message.TimeToLive is > 0 and < uint.MaxValue ? TimeSpan.FromMilliseconds(message.TimeToLive) : null,
                Durable = message.Durable,
                MessageId = ValueConversions.TryGuidFromString(message.MessageId?.ToString()) ?? Guid.CreateVersion7(),
                SentTimestamp = ValueConversions.TryDateTimeOffsetFromString(message.GetProperty(Constants.SendTimestampHeaderKey)?.ToString()),
                PayloadType = message.GetProperty(Constants.PayloadTypeHeaderKey)?.ToString(),
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
