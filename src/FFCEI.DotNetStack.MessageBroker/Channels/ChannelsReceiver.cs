using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;
using System.Threading.Channels;

namespace FFCEI.DotNetStack.MessageBroker.Channels;

public sealed class ChannelsReceiver(string subject, WeakReference<ChannelsConnector> connector)
    : Receiver(MessageBrokerProviderType.Channels, subject)
{
    private Channel<MessageEnvelope>? _channel;
    private Task? _receivingTask;

    public WeakReference<ChannelsConnector> Connector { get; private set; } = connector;
    public ChannelReader<MessageEnvelope>? Reader => _channel?.Reader;

    protected override async Task DoConfigureAsync(MessageBrokerAddress address, CancellationToken cancellationToken = default)
    {
        var hasConnector = Connector.TryGetTarget(out var connector) && connector != null;

        ObjectDisposedException.ThrowIf(!hasConnector, this);

        ArgumentNullException.ThrowIfNull(address, nameof(address));

        _channel = connector!.ChannelManager.Start(Subject, address) ??
            throw new InvalidOperationException($"ChannelManager returned no channel for subject '{Subject}' / address '{address}'");

        Address = address;
    }

    protected override async Task DoStartAsync(CancellationToken cancellationToken = default)
    {
        CancellationTokenSourceFactory = new(cancellationToken);

        var token = CancellationTokenSourceFactory.Token;

        _receivingTask = Task.Run(async () => await ReceiveLoopAsync(token).ConfigureAwait(false), token);
    }

    protected override Task DoStopAsync()
    {
        return ExceptionProtector.ExecuteTaskAsync(_receivingTask ?? Task.CompletedTask);
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var messageEnvelope in _channel!.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    if (DeserializeMessageEnvelope(messageEnvelope) && !messageEnvelope.Expired())
                    {
                        await OnReceivedAsync(messageEnvelope, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (DeserializeMessageEnvelopeException)
                {
                    continue;
                }
                catch (MessageHandlingAlreadyReportedException)
                {
                    continue;
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    if (OnMessageHandlingException != null)
                    {
                        ExceptionProtector.ExecuteAction(() => OnMessageHandlingException.Invoke(exception, messageEnvelope));
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
