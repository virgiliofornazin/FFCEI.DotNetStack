using Apache.Qpid.Proton.Client;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;

namespace FFCEI.DotNetStack.MessageBroker.Qpid;

public sealed class QpidReceiver(string subject, WeakReference<QpidConnector> connector)
    : Receiver(MessageBrokerProviderType.Qpid, subject)
{
    private static readonly TimeSpan _receivePollTimeout = TimeSpan.FromMilliseconds(500);

    private Task? _receivingTask;
    private IReceiver? _receiverLink;

    public WeakReference<QpidConnector> Connector { get; private set; } = connector;

    protected override Task DoConfigureAsync(MessageBrokerAddress address, CancellationToken cancellationToken = default)
    {
        Address = address;

        return Task.CompletedTask;
    }

    protected override async Task DoStartAsync(CancellationToken cancellationToken = default)
    {
        if (Address == null)
        {
            throw new InvalidOperationException("QpidReceiver not configured");
        }

        var hasConnector = Connector.TryGetTarget(out var connector) && (connector != null);

        ObjectDisposedException.ThrowIf(!hasConnector, this);

        var addressString = QpidConnector.ExtractQpidAddress(Address);

        CancellationTokenSourceFactory = new CancellationTokenSourceFactory(cancellationToken);

        var token = CancellationTokenSourceFactory.TokenSource.Token;

        _receiverLink = await connector!.OpenReceiverLinkAsync(addressString, cancellationToken).ConfigureAwait(false);

        _receivingTask = Task.Run(async () => await ReceiveLoopAsync(token).ConfigureAwait(false), token);
    }

    protected override async Task DoStopAsync()
    {
        await ExceptionProtector.ExecuteTaskAsync(_receivingTask ?? Task.CompletedTask).ConfigureAwait(false);

        _receivingTask = null;

        if (_receiverLink != null)
        {
            var hasConnector = Connector.TryGetTarget(out var connector) && (connector != null);

            if (hasConnector)
            {
                await connector!.CloseReceiverLinkAsync(_receiverLink).ConfigureAwait(false);
            }

            _receiverLink = null;
        }
    }

    private static byte[] ExtractBinaryPayload(object? messageBody)
    {
        if (messageBody is byte[] payloadBytes)
        {
            return payloadBytes;
        }

        if (messageBody is Apache.Qpid.Proton.Buffer.IProtonBuffer payloadBuffer)
        {
            var buffer = new byte[payloadBuffer.ReadableBytes];

            _ = payloadBuffer.CopyInto(payloadBuffer.ReadOffset, buffer, 0, buffer.Length);

            return buffer;
        }

        throw new DeserializeMessageEnvelopeException(new InvalidOperationException("AMQP message body is not a binary payload"));
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                IDelivery? delivery = null;
                MessageEnvelope? messageEnvelope = null;
                var attemptNumber = 1;

                try
                {
                    var receiverLink = _receiverLink ?? await ReopenReceiverLinkAsync(cancellationToken).ConfigureAwait(false);

                    delivery = await receiverLink.ReceiveAsync(_receivePollTimeout).WaitAsync(cancellationToken).ConfigureAwait(false);

                    if (delivery == null)
                    {
                        continue;
                    }

                    var message = delivery.Message();

                    attemptNumber = (int)message.DeliveryCount + 1;

                    var messageEnvelopeAttributes = QpidConnector.MessageAttributesFromQpidProtonMessage(message);

                    messageEnvelope = new MessageEnvelope()
                    {
                        Attributes = messageEnvelopeAttributes,
                        EncodedPayload = ExtractBinaryPayload(message.Body)
                    };

                    if (DeserializeMessageEnvelope(messageEnvelope) && !messageEnvelope.Expired())
                    {
                        await OnReceivedAsync(messageEnvelope, cancellationToken).ConfigureAwait(false);
                    }

                    await ExceptionProtector.ExecuteTaskAsync(delivery.AcceptAsync()).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (DeserializeMessageEnvelopeException)
                {
                    if (delivery != null)
                    {
                        await ExceptionProtector.ExecuteTaskAsync(delivery.RejectAsync("ffcei:deserialize-failed", "Message envelope could not be deserialized")).ConfigureAwait(false);
                    }
                }
                catch (MessageHandlingAlreadyReportedException)
                {
                    if (delivery != null)
                    {
                        await ExceptionProtector.ExecuteTaskAsync(delivery.RejectAsync("ffcei:handling-failed", "Message handling failed and was reported")).ConfigureAwait(false);
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    if (delivery != null)
                    {
                        if (attemptNumber >= RetryMessageCount)
                        {
                            await ExceptionProtector.ExecuteTaskAsync(delivery.RejectAsync("ffcei:handling-failed", "Message handling failed after retry")).ConfigureAwait(false);
                        }
                        else
                        {
                            await ExceptionProtector.ExecuteTaskAsync(delivery.ModifiedAsync(deliveryFailed: true, undeliverableHere: false)).ConfigureAwait(false);
                        }
                    }

                    if (OnMessageHandlingException != null)
                    {
                        ExceptionProtector.ExecuteAction(() => OnMessageHandlingException.Invoke(exception, messageEnvelope));
                    }

                    if (delivery == null)
                    {
                        _receiverLink = null;

                        await WaitAfterErrorAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task<IReceiver> ReopenReceiverLinkAsync(CancellationToken cancellationToken)
    {
        var hasConnector = Connector.TryGetTarget(out var connector) && (connector != null);

        ObjectDisposedException.ThrowIf(!hasConnector, this);

        ArgumentNullException.ThrowIfNull(Address, nameof(Address));

        var addressString = QpidConnector.ExtractQpidAddress(Address);

        _receiverLink = await connector!.OpenReceiverLinkAsync(addressString, cancellationToken).ConfigureAwait(false);

        return _receiverLink;
    }
}
