using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;
using Google.Cloud.PubSub.V1;
using System.Collections.Concurrent;

namespace FFCEI.DotNetStack.MessageBroker.GooglePubSub;

public sealed class GooglePubSubReceiver(string subject, WeakReference<GooglePubSubConnector> connector)
    : Receiver(MessageBrokerProviderType.GooglePubSub, subject)
{
    private const int MaxMessagesPerPull = 10;

    private Task? _receivingTask;
    private SubscriptionName? _subscriptionName;
    private bool _hasDeadLetterPolicy;
    private readonly ConcurrentDictionary<string, int> _failureCounts = [];

    public WeakReference<GooglePubSubConnector> Connector { get; private set; } = connector;

    protected override Task DoConfigureAsync(MessageBrokerAddress address, CancellationToken cancellationToken = default)
    {
        _ = GooglePubSubConnector.ExtractTopicId(address);

        Address = address;

        return Task.CompletedTask;
    }

    protected override Task DoStartAsync(CancellationToken cancellationToken = default)
    {
        if (Address == null)
        {
            throw new InvalidOperationException("GooglePubSubReceiver not configured");
        }

        var hasConnector = Connector.TryGetTarget(out var connector) && (connector != null);

        ObjectDisposedException.ThrowIf(!hasConnector, this);

        _subscriptionName = connector!.GetSubscriptionName(Address, Subject);

        return StartReceivingAsync(connector, cancellationToken);
    }

    private async Task StartReceivingAsync(GooglePubSubConnector connector, CancellationToken cancellationToken)
    {
        _hasDeadLetterPolicy = await connector.HasDeadLetterPolicyAsync(_subscriptionName!, cancellationToken).ConfigureAwait(false);

        CancellationTokenSourceFactory = new CancellationTokenSourceFactory(cancellationToken);

        var token = CancellationTokenSourceFactory.TokenSource.Token;

        _receivingTask = Task.Run(async () => await ReceiveLoopAsync(connector, _subscriptionName!, token).ConfigureAwait(false), token);
    }

    protected override async Task DoStopAsync()
    {
        await ExceptionProtector.ExecuteTaskAsync(_receivingTask ?? Task.CompletedTask).ConfigureAwait(false);

        _receivingTask = null;
        _subscriptionName = null;
        _hasDeadLetterPolicy = false;
    }

    private async Task ReceiveLoopAsync(GooglePubSubConnector connector, SubscriptionName subscriptionName, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var response = await connector.SubscriberApi.PullAsync(subscriptionName, MaxMessagesPerPull, cancellationToken).ConfigureAwait(false);

                    if (response.ReceivedMessages.Count == 0)
                    {
                        await Task.Delay(PollWaitInterval, cancellationToken).ConfigureAwait(false);

                        continue;
                    }

                    foreach (var receivedMessage in response.ReceivedMessages)
                    {
                        await ProcessReceivedMessageAsync(connector, subscriptionName, receivedMessage, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    if (OnMessageHandlingException != null)
                    {
                        ExceptionProtector.ExecuteAction(() => OnMessageHandlingException.Invoke(exception, null));
                    }

                    await WaitAfterErrorAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task ProcessReceivedMessageAsync(GooglePubSubConnector connector, SubscriptionName subscriptionName, ReceivedMessage receivedMessage, CancellationToken cancellationToken)
    {
        MessageEnvelope? messageEnvelope = null;

        try
        {
            var messageEnvelopeAttributes = GooglePubSubConnector.MessageAttributesFromPubsubMessage(receivedMessage.Message);

            messageEnvelope = new MessageEnvelope()
            {
                Attributes = messageEnvelopeAttributes,
                EncodedPayload = receivedMessage.Message.Data.ToByteArray()
            };

            if (DeserializeMessageEnvelope(messageEnvelope) && !messageEnvelope.Expired())
            {
                await OnReceivedAsync(messageEnvelope, cancellationToken).ConfigureAwait(false);
            }

            await ExceptionProtector.ExecuteTaskAsync(connector.SubscriberApi.AcknowledgeAsync(subscriptionName, [receivedMessage.AckId], CancellationToken.None)).ConfigureAwait(false);

            _ = _failureCounts.TryRemove(receivedMessage.Message.MessageId, out int _);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DeserializeMessageEnvelopeException)
        {
            await ExceptionProtector.ExecuteTaskAsync(connector.SubscriberApi.AcknowledgeAsync(subscriptionName, [receivedMessage.AckId], CancellationToken.None)).ConfigureAwait(false);
        }
        catch (MessageHandlingAlreadyReportedException)
        {
            await ExceptionProtector.ExecuteTaskAsync(connector.SubscriberApi.AcknowledgeAsync(subscriptionName, [receivedMessage.AckId], CancellationToken.None)).ConfigureAwait(false);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            var giveUp = false;

            if (!_hasDeadLetterPolicy)
            {
                var failureCount = _failureCounts.AddOrUpdate(receivedMessage.Message.MessageId, 1, (_, existing) => existing + 1);

                giveUp = failureCount >= RetryMessageCount;

                if (giveUp)
                {
                    _ = _failureCounts.TryRemove(receivedMessage.Message.MessageId, out int _);
                }
            }

            if (giveUp)
            {
                await ExceptionProtector.ExecuteTaskAsync(connector.SubscriberApi.AcknowledgeAsync(subscriptionName, [receivedMessage.AckId], CancellationToken.None)).ConfigureAwait(false);
            }
            else
            {
                await ExceptionProtector.ExecuteTaskAsync(connector.SubscriberApi.ModifyAckDeadlineAsync(subscriptionName, [receivedMessage.AckId], 0, CancellationToken.None)).ConfigureAwait(false);
            }

            if (OnMessageHandlingException != null)
            {
                ExceptionProtector.ExecuteAction(() => OnMessageHandlingException.Invoke(exception, messageEnvelope));
            }
        }
    }
}
