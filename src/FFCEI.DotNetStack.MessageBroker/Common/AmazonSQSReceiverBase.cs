using Amazon.SQS;
using Amazon.SQS.Model;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.AmazonSQS;
using System.Globalization;

namespace FFCEI.DotNetStack.MessageBroker.Common;

public abstract class AmazonSQSReceiverBase<TConnector>(
    MessageBrokerProviderType messageBrokerProvider, string subject, WeakReference<TConnector> connector)
    : Receiver(messageBrokerProvider, subject) where TConnector : class
{
    private const int MaxMessagesPerPoll = 10;
    private const int LongPollWaitSeconds = 60;

    private Task? _receivingTask;
    private string? _queueUrl;
    private bool _hasRedrivePolicy;

    public WeakReference<TConnector> Connector { get; private set; } = connector;

    protected abstract void ValidateAddress(MessageBrokerAddress address);

    protected abstract IAmazonSQS SqsClientOf(TConnector connector);

    protected abstract Task<string> ResolveQueueUrlAsync(TConnector connector, MessageBrokerAddress address, CancellationToken cancellationToken);

#pragma warning disable CA1054 // URI-like parameters should not be strings
    protected abstract Task<bool> HasRedrivePolicyAsync(TConnector connector, string queueUrl, CancellationToken cancellationToken);
#pragma warning restore CA1054 // URI-like parameters should not be strings

    protected override Task DoConfigureAsync(MessageBrokerAddress address, CancellationToken cancellationToken = default)
    {
        ValidateAddress(address);

        Address = address;

        return Task.CompletedTask;
    }

    protected override async Task DoStartAsync(CancellationToken cancellationToken = default)
    {
        if (Address == null)
        {
            throw new InvalidOperationException($"{GetType().Name} not configured");
        }

        var hasConnector = Connector.TryGetTarget(out var connector) && (connector != null);

        ObjectDisposedException.ThrowIf(!hasConnector, this);

        _queueUrl = await ResolveQueueUrlAsync(connector!, Address, cancellationToken).ConfigureAwait(false);
        _hasRedrivePolicy = await HasRedrivePolicyAsync(connector!, _queueUrl, cancellationToken).ConfigureAwait(false);

        CancellationTokenSourceFactory = new CancellationTokenSourceFactory(cancellationToken);

        var token = CancellationTokenSourceFactory.TokenSource.Token;

        _receivingTask = Task.Run(async () => await ReceiveLoopAsync(connector!, _queueUrl, token).ConfigureAwait(false), token);
    }

    protected override async Task DoStopAsync()
    {
        await ExceptionProtector.ExecuteTaskAsync(_receivingTask ?? Task.CompletedTask).ConfigureAwait(false);

        _receivingTask = null;
        _queueUrl = null;
        _hasRedrivePolicy = false;
    }

    private async Task ReceiveLoopAsync(TConnector connector, string queueUrl, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var request = new ReceiveMessageRequest
                    {
                        QueueUrl = queueUrl,
                        MaxNumberOfMessages = MaxMessagesPerPoll,
                        WaitTimeSeconds = LongPollWaitSeconds,
                        MessageAttributeNames = ["All"],
                        MessageSystemAttributeNames = ["ApproximateReceiveCount"]
                    };

                    var response = await SqsClientOf(connector).ReceiveMessageAsync(request, cancellationToken).ConfigureAwait(false);

                    if ((response.Messages == null) || (response.Messages.Count == 0))
                    {
                        continue;
                    }

                    foreach (var message in response.Messages)
                    {
                        await ProcessReceivedMessageAsync(connector, queueUrl, message, cancellationToken).ConfigureAwait(false);
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

    private async Task ProcessReceivedMessageAsync(TConnector connector, string queueUrl, Message message, CancellationToken cancellationToken)
    {
        MessageEnvelope? messageEnvelope = null;

        var receiveCount = (message.Attributes != null) &&
            message.Attributes.TryGetValue("ApproximateReceiveCount", out var receiveCountValue) &&
            int.TryParse(receiveCountValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedReceiveCount) ? parsedReceiveCount : 1;

        try
        {
            var messageEnvelopeAttributes = AmazonSQSConnector.MessageAttributesFromSQSMessage(message);

            messageEnvelope = new MessageEnvelope()
            {
                Attributes = messageEnvelopeAttributes,
                EncodedPayload = Convert.FromBase64String(message.Body)
            };

            if (DeserializeMessageEnvelope(messageEnvelope) && !messageEnvelope.Expired())
            {
                await OnReceivedAsync(messageEnvelope, cancellationToken).ConfigureAwait(false);
            }

            _ = await SqsClientOf(connector).DeleteMessageAsync(queueUrl, message.ReceiptHandle, CancellationToken.None).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DeserializeMessageEnvelopeException)
        {
            _ = await ExceptionProtector.ExecuteTaskAndReturnOrNullReference(SqsClientOf(connector).DeleteMessageAsync(queueUrl, message.ReceiptHandle, CancellationToken.None)).ConfigureAwait(false);
        }
        catch (MessageHandlingAlreadyReportedException)
        {
            _ = await ExceptionProtector.ExecuteTaskAndReturnOrNullReference(SqsClientOf(connector).DeleteMessageAsync(queueUrl, message.ReceiptHandle, CancellationToken.None)).ConfigureAwait(false);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            if (!_hasRedrivePolicy && (receiveCount >= RetryMessageCount))
            {
                _ = await ExceptionProtector.ExecuteTaskAndReturnOrNullReference(SqsClientOf(connector).DeleteMessageAsync(queueUrl, message.ReceiptHandle, CancellationToken.None)).ConfigureAwait(false);
            }
            else
            {
                _ = await ExceptionProtector.ExecuteTaskAndReturnOrNullReference(SqsClientOf(connector).ChangeMessageVisibilityAsync(queueUrl, message.ReceiptHandle, 0, CancellationToken.None)).ConfigureAwait(false);
            }

            if (OnMessageHandlingException != null)
            {
                ExceptionProtector.ExecuteAction(() => OnMessageHandlingException.Invoke(exception, messageEnvelope));
            }
        }
    }
}
