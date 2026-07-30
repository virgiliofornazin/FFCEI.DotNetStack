using Azure.Messaging.ServiceBus;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;

namespace FFCEI.DotNetStack.MessageBroker.AzureServiceBus;

public sealed class AzureServiceBusReceiver(string subject, WeakReference<AzureServiceBusConnector> connector)
    : Receiver(MessageBrokerProviderType.AzureServiceBus, subject)
{
    private static readonly TimeSpan _receivePollTimeout = TimeSpan.FromSeconds(60);

    private Task? _receivingTask;
#pragma warning disable CA2213 // Disposable fields should be disposed
    private ServiceBusReceiver? _receiverLink;
#pragma warning restore CA2213 // Disposable fields should be disposed

    public WeakReference<AzureServiceBusConnector> Connector { get; private set; } = connector;

    protected override Task DoConfigureAsync(MessageBrokerAddress address, CancellationToken cancellationToken = default)
    {
        _ = AzureServiceBusConnector.ExtractEntityName(address);

        Address = address;

        return Task.CompletedTask;
    }

    protected override Task DoStartAsync(CancellationToken cancellationToken = default)
    {
        if (Address == null)
        {
            throw new InvalidOperationException("AzureServiceBusReceiver not configured");
        }

        var hasConnector = Connector.TryGetTarget(out var connector) && (connector != null);

        ObjectDisposedException.ThrowIf(!hasConnector, this);

        CancellationTokenSourceFactory = new CancellationTokenSourceFactory(cancellationToken);

        var token = CancellationTokenSourceFactory.TokenSource.Token;

        _receiverLink = connector!.OpenReceiverLink(Address, Subject);

        _receivingTask = Task.Run(async () => await ReceiveLoopAsync(token).ConfigureAwait(false), token);

        return Task.CompletedTask;
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

            await _receiverLink.DisposeObjectSafelyIfNeededAsync().ConfigureAwait(false);

            _receiverLink = null;
        }
    }

    protected override void OnDispose()
        => _receiverLink.DisposeObjectSafelyIfNeeded();

    protected override ValueTask OnDisposeAsync()
        => _receiverLink.DisposeSafelyIfNeededAsync();

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ServiceBusReceivedMessage? message = null;
                MessageEnvelope? messageEnvelope = null;
                ServiceBusReceiver? receiverLink = null;
                var attemptNumber = 1;

                try
                {
                    receiverLink = EnsureReceiverLinkOpen();

                    message = await receiverLink.ReceiveMessageAsync(_receivePollTimeout, cancellationToken).ConfigureAwait(false);

                    if (message == null)
                    {
                        continue;
                    }

                    attemptNumber = message.DeliveryCount;

                    var messageEnvelopeAttributes = AzureServiceBusConnector.MessageAttributesFromServiceBusMessage(message);

                    messageEnvelope = new MessageEnvelope()
                    {
                        Attributes = messageEnvelopeAttributes,
                        EncodedPayload = message.Body.ToArray()
                    };

                    if (DeserializeMessageEnvelope(messageEnvelope) && !messageEnvelope.Expired())
                    {
                        await OnReceivedAsync(messageEnvelope, cancellationToken).ConfigureAwait(false);
                    }

                    await ExceptionProtector.ExecuteTaskAsync(receiverLink.CompleteMessageAsync(message, CancellationToken.None)).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (DeserializeMessageEnvelopeException)
                {
                    if (message != null)
                    {
                        await ExceptionProtector.ExecuteTaskAsync(receiverLink!.DeadLetterMessageAsync(message, "ffcei:deserialize-failed", "Message envelope could not be deserialized", CancellationToken.None)).ConfigureAwait(false);
                    }
                }
                catch (MessageHandlingAlreadyReportedException)
                {
                    if (message != null)
                    {
                        await ExceptionProtector.ExecuteTaskAsync(receiverLink!.DeadLetterMessageAsync(message, "ffcei:handling-failed", "Message handling failed and was reported", CancellationToken.None)).ConfigureAwait(false);
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    if (message != null)
                    {
                        if (attemptNumber >= RetryMessageCount)
                        {
                            await ExceptionProtector.ExecuteTaskAsync(receiverLink!.DeadLetterMessageAsync(message, "ffcei:handling-failed", "Message handling failed after retry", CancellationToken.None)).ConfigureAwait(false);
                        }
                        else
                        {
                            await ExceptionProtector.ExecuteTaskAsync(receiverLink!.AbandonMessageAsync(message, cancellationToken: CancellationToken.None)).ConfigureAwait(false);
                        }
                    }

                    if (OnMessageHandlingException != null)
                    {
                        ExceptionProtector.ExecuteAction(() => OnMessageHandlingException.Invoke(exception, messageEnvelope));
                    }

                    if (message == null)
                    {
                        await WaitAfterErrorAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private ServiceBusReceiver EnsureReceiverLinkOpen()
    {
        if ((_receiverLink != null) && !_receiverLink.IsClosed)
        {
            return _receiverLink;
        }

        var hasConnector = Connector.TryGetTarget(out var connector) && (connector != null);

        ObjectDisposedException.ThrowIf(!hasConnector, this);

        ArgumentNullException.ThrowIfNull(Address, nameof(Address));

        _receiverLink = connector!.OpenReceiverLink(Address, Subject);

        return _receiverLink;
    }
}
