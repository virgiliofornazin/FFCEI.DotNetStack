using Confluent.Kafka;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;

namespace FFCEI.DotNetStack.MessageBroker.Kafka;

public sealed class KafkaReceiver(IConsumer<Ignore, byte[]> consumer, string subject, WeakReference<KafkaConnector> connector)
    : Receiver(MessageBrokerProviderType.Kafka, subject)
{
    private Task? _receivingTask;
    private readonly Dictionary<TopicPartition, (long Offset, int FailureCount)> _lastFailedOffsets = [];
    private bool _stopped;

    public WeakReference<KafkaConnector> Connector { get; private set; } = connector;
    public IConsumer<Ignore, byte[]> Consumer { get; private set; } = consumer;

    protected override Task DoConfigureAsync(MessageBrokerAddress address, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(Consumer, nameof(Consumer));

        var topic = KafkaConnector.ExtractKakfaTopicFromAddress(address);

        Consumer.Subscribe(topic);

        Address = address;

        return Task.CompletedTask;
    }

    protected override Task DoStartAsync(CancellationToken cancellationToken = default)
    {
        if (Address == null)
        {
            throw new InvalidOperationException("KafkaReceiver not configured");
        }

        if (_stopped)
        {
            var hasConnector = Connector.TryGetTarget(out var connector) && (connector != null);

            ObjectDisposedException.ThrowIf(!hasConnector, this);

#pragma warning disable CA2000 // Dispose objects before losing scope
            Consumer = connector!.CreateConsumerForReceiver(Subject);
#pragma warning restore CA2000 // Dispose objects before losing scope

            Consumer.Subscribe(KafkaConnector.ExtractKakfaTopicFromAddress(Address));

            _stopped = false;
        }

        CancellationTokenSourceFactory = new CancellationTokenSourceFactory(cancellationToken);

        var token = CancellationTokenSourceFactory.TokenSource.Token;

        _receivingTask = Task.Run(async () => await ReceiveLoopAsync(token).ConfigureAwait(false), token);

        return Task.CompletedTask;
    }

    protected override async Task DoStopAsync()
    {
        await ExceptionProtector.ExecuteTaskAsync(_receivingTask ?? Task.CompletedTask).ConfigureAwait(false);

        await ExceptionProtector.ExecuteTaskFunctionAsync(async () =>
        {
            var hasConnector = Connector.TryGetTarget(out var connector) && connector != null;

            if (hasConnector)
            {
                await connector!.CloseConsumerAsync(Consumer).ConfigureAwait(false);
            }
        })
        .ConfigureAwait(false);

        _stopped = true;
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<Ignore, byte[]>? consumeResult = null;
                MessageEnvelope? messageEnvelope = null;

                try
                {
                    consumeResult = Consumer.Consume(cancellationToken);

                    if ((consumeResult == null) || consumeResult.IsPartitionEOF || (consumeResult.Message == null))
                    {
                        continue;
                    }

                    var messageEnvelopeAttributes = KafkaConnector.MessageAttributesFromConsumeResult(consumeResult);

                    messageEnvelope = new MessageEnvelope()
                    {
                        Attributes = messageEnvelopeAttributes,
                        EncodedPayload = consumeResult.Message.Value
                    };

                    if (DeserializeMessageEnvelope(messageEnvelope) && !messageEnvelope.Expired())
                    {
                        await OnReceivedAsync(messageEnvelope, cancellationToken).ConfigureAwait(false);
                    }

                    Consumer.Commit(consumeResult);

                    _ = _lastFailedOffsets.Remove(consumeResult.TopicPartition);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (DeserializeMessageEnvelopeException)
                {
                    if (consumeResult != null)
                    {
                        ExceptionProtector.ExecuteAction(() => Consumer.Commit(consumeResult));

                        _ = _lastFailedOffsets.Remove(consumeResult.TopicPartition);
                    }
                }
                catch (MessageHandlingAlreadyReportedException)
                {
                    if (consumeResult != null)
                    {
                        ExceptionProtector.ExecuteAction(() => Consumer.Commit(consumeResult));

                        _ = _lastFailedOffsets.Remove(consumeResult.TopicPartition);
                    }

                    await WaitAfterErrorAsync(cancellationToken).ConfigureAwait(false);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    if (consumeResult != null)
                    {
                        var previousFailureCount = _lastFailedOffsets.TryGetValue(consumeResult.TopicPartition, out var previousFailure) &&
                            (previousFailure.Offset == consumeResult.Offset.Value) ? previousFailure.FailureCount : 0;

                        var failureCount = previousFailureCount + 1;

                        if (failureCount >= RetryMessageCount)
                        {
                            ExceptionProtector.ExecuteAction(() => Consumer.Commit(consumeResult));

                            _ = _lastFailedOffsets.Remove(consumeResult.TopicPartition);
                        }
                        else
                        {
                            _lastFailedOffsets[consumeResult.TopicPartition] = (consumeResult.Offset.Value, failureCount);

                            try
                            {
                                Consumer.Seek(consumeResult.TopicPartitionOffset);
                            }
                            catch (KafkaException seekException)
                            {
                                if (Connector.TryGetTarget(out var connector) && (connector != null))
                                {
                                    MessageBrokerLog.SeekFailed(connector.Logger, seekException, MessageBrokerProvider,
                                        consumeResult.TopicPartition.ToString(), consumeResult.Offset.Value);
                                }
                            }
                        }
                    }

                    if (OnMessageHandlingException != null)
                    {
                        ExceptionProtector.ExecuteAction(() => OnMessageHandlingException.Invoke(exception, messageEnvelope));
                    }

                    await WaitAfterErrorAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
