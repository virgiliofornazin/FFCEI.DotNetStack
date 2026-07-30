using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;
using System.Threading.Channels;

namespace FFCEI.DotNetStack.MessageBroker.Channels;

public sealed class ChannelsSender(string subject, WeakReference<ChannelsConnector> connector)
    : Sender(MessageBrokerProviderType.Channels, subject)
{
    private MessageBrokerAddress? _lastAddress;
    private Channel<MessageEnvelope>? _lastChannelSent;

    public WeakReference<ChannelsConnector> Connector { get; private set; } = connector;

    protected override async Task DoSendToAsync(MessageBrokerAddress address, MessageEnvelope messageEnvelope, CancellationToken cancellationToken = default)
    {
        IEnumerable<Channel<MessageEnvelope>>? targets = null;

        var channel = address.Type == MessageBrokerAddressType.Fanout || _lastAddress == null || _lastAddress != address || (_lastChannelSent?.Reader.Completion.IsCompleted ?? true)
            ? null : _lastChannelSent;

        if (channel == null)
        {
            var hasConnector = Connector.TryGetTarget(out var connector) && connector != null;

            ObjectDisposedException.ThrowIf(!hasConnector, this);

            channel = connector!.ChannelManager.Start(Subject, address) ??
                throw new InvalidOperationException($"ChannelManager returned no channel for subject '{Subject}' / address '{address}'");

            if (address.Type == MessageBrokerAddressType.Fanout)
            {
                targets = connector!.ChannelManager.Channels;
            }
        }

        if (address.Type != MessageBrokerAddressType.Fanout)
        {
            if (channel != _lastChannelSent)
            {
                _lastAddress = address;
                _lastChannelSent = channel;
            }

            targets = [channel];
        }

        using var cancellationTokenSourceFactory = new CancellationTokenSourceFactory(cancellationToken);

        var token = cancellationTokenSourceFactory.Token;

        try
        {
            CancellationTokenSourceFactory = cancellationTokenSourceFactory;

            messageEnvelope.SerializeEnvelope(Serializer);
            messageEnvelope.Payload = null;

            var targetList = targets?.ToList() ?? [];

            for (var index = 0; index < targetList.Count; index++)
            {
                var envelopeForTarget = index > 0 ? messageEnvelope.Clone() : messageEnvelope;

                await targetList[index].Writer.WriteAsync(envelopeForTarget, token).ConfigureAwait(false);
            }
        }
        finally
        {
            CancellationTokenSourceFactory = null;
        }
    }
}
