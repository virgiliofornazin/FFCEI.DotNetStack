using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;

namespace FFCEI.DotNetStack.MessageBroker.RabbitMQ;

public sealed class RabbitMQReceiver(IChannel channel, string subject, WeakReference<RabbitMQConnector> connector)
    : Receiver(MessageBrokerProviderType.RabbitMQ, subject)
{
    private Task? _receivingTask;
    private string? _consumerTag;
    private readonly ConcurrentDictionary<string, int> _failureCounts = [];

    public WeakReference<RabbitMQConnector> Connector { get; private set; } = connector;
    public IChannel Channel { get; private set; } = channel!;

    protected override Task DoConfigureAsync(MessageBrokerAddress address, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(Channel, nameof(Channel));

        if (address.Type != MessageBrokerAddressType.Queue)
        {
            throw new InvalidOperationException("RabbitMQ only can receive from queues");
        }

        Address = address;

        return Task.CompletedTask;
    }

    protected override async Task DoStartAsync(CancellationToken cancellationToken = default)
    {
        if (!Channel.IsOpen)
        {
            var hasConnector = Connector.TryGetTarget(out var connector) && (connector != null);

            ObjectDisposedException.ThrowIf(!hasConnector, this);

            Channel = await connector!.CreateChannelForReceiverAsync(cancellationToken).ConfigureAwait(false);
        }

        CancellationTokenSourceFactory = new CancellationTokenSourceFactory(cancellationToken);

        var token = CancellationTokenSourceFactory.TokenSource.Token;
        var consumer = new AsyncEventingBasicConsumer(Channel);

        _consumerTag = null;
        _receivingTask = Task.Run(async () => await ReceiveLoopAsync(consumer, token).ConfigureAwait(false), token);
    }

    protected override async Task DoStopAsync()
    {
        if (_consumerTag != null)
        {
            await ExceptionProtector.ExecuteTaskAsync(Channel.BasicCancelAsync(_consumerTag, noWait: false, CancellationToken.None)).ConfigureAwait(false);

            _consumerTag = null;
        }

        var hasConnector = Connector.TryGetTarget(out var connector);

        if (hasConnector && (connector != null))
        {
            await connector.CloseChannelAsync(Channel).ConfigureAwait(false);
        }

        await ExceptionProtector.ExecuteTaskAsync(_receivingTask ?? Task.CompletedTask).ConfigureAwait(false);
    }

    private async Task ReceiveLoopAsync(AsyncEventingBasicConsumer consumer, CancellationToken cancellationToken)
    {
        try
        {
            consumer.ReceivedAsync += async (_, eventArguments) =>
            {
                MessageEnvelope? messageEnvelope = null;

                try
                {
                    var messageEnvelopeAttributes = RabbitMQConnector.MessageAttributesFromBasicProperties(eventArguments.BasicProperties);

                    messageEnvelope = new MessageEnvelope()
                    {
                        Attributes = messageEnvelopeAttributes,
                        EncodedPayload = eventArguments.Body
                    };

                    if (DeserializeMessageEnvelope(messageEnvelope) && !messageEnvelope.Expired())
                    {
                        await OnReceivedAsync(messageEnvelope, cancellationToken).ConfigureAwait(false);
                    }

                    await ExceptionProtector.ExecuteValueTaskAsync(Channel.BasicAckAsync(eventArguments.DeliveryTag, multiple: false, CancellationToken.None)).ConfigureAwait(false);

                    if (!string.IsNullOrWhiteSpace(eventArguments.BasicProperties.MessageId))
                    {
                        _ = _failureCounts.TryRemove(eventArguments.BasicProperties.MessageId, out int _);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (DeserializeMessageEnvelopeException)
                {
                    await ExceptionProtector.ExecuteValueTaskAsync(Channel.BasicRejectAsync(eventArguments.DeliveryTag, requeue: false, CancellationToken.None)).ConfigureAwait(false);
                }
                catch (MessageHandlingAlreadyReportedException)
                {
                    await ExceptionProtector.ExecuteValueTaskAsync(Channel.BasicRejectAsync(eventArguments.DeliveryTag, requeue: false, CancellationToken.None)).ConfigureAwait(false);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    var messageId = eventArguments.BasicProperties.MessageId;
                    var failureCount = !string.IsNullOrWhiteSpace(messageId) ? _failureCounts.AddOrUpdate(messageId, 1, (_, existing) => existing + 1) : 1;
                    var requeue = failureCount < RetryMessageCount;

                    if (!requeue && !string.IsNullOrWhiteSpace(messageId))
                    {
                        _ = _failureCounts.TryRemove(messageId, out int _);
                    }

                    await ExceptionProtector.ExecuteValueTaskAsync(Channel.BasicNackAsync(eventArguments.DeliveryTag, multiple: false, requeue: requeue, CancellationToken.None)).ConfigureAwait(false);

                    if (OnMessageHandlingException != null)
                    {
                        ExceptionProtector.ExecuteAction(() => OnMessageHandlingException.Invoke(exception, messageEnvelope));
                    }

                    await WaitAfterErrorAsync(cancellationToken).ConfigureAwait(false);
                }
            };

            ArgumentNullException.ThrowIfNull(Address, nameof(Address));

            if (Address.Type != MessageBrokerAddressType.Queue)
            {
                throw new InvalidOperationException("RabbitMQ only can receive from queues");
            }

            _consumerTag = await Channel.BasicConsumeAsync(queue: Address.Value, autoAck: false, consumer: consumer, cancellationToken: cancellationToken).ConfigureAwait(false);

            await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }
}
