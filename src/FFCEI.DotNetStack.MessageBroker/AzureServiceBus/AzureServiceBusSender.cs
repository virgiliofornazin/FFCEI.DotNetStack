using Azure.Messaging.ServiceBus;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;

namespace FFCEI.DotNetStack.MessageBroker.AzureServiceBus;

public sealed class AzureServiceBusSender(string subject, WeakReference<AzureServiceBusConnector> connector)
    : Sender(MessageBrokerProviderType.AzureServiceBus, subject)
{
    private readonly Dictionary<string, ServiceBusSender> _senderLinks = [];
#pragma warning disable CA2213 // Disposable fields should be disposed
    private readonly SemaphoreSlim _senderLinksLock = new(1, 1);
#pragma warning restore CA2213 // Disposable fields should be disposed

    public WeakReference<AzureServiceBusConnector> Connector { get; private set; } = connector;

    protected override async Task DoSendToAsync(MessageBrokerAddress address, MessageEnvelope messageEnvelope, CancellationToken cancellationToken = default)
    {
        var entityName = AzureServiceBusConnector.ExtractEntityName(address);

        using var cancellationTokenSourceFactory = new CancellationTokenSourceFactory(cancellationToken);

        var token = cancellationTokenSourceFactory.Token;

        try
        {
            CancellationTokenSourceFactory = cancellationTokenSourceFactory;

            var senderLink = await GetOrOpenSenderLinkAsync(entityName, token).ConfigureAwait(false);

            messageEnvelope.SerializeEnvelope(Serializer);

            var message = AzureServiceBusConnector.MessageAttributesToServiceBusMessage(messageEnvelope);

            await senderLink.SendMessageAsync(message, token).ConfigureAwait(false);
        }
        finally
        {
            CancellationTokenSourceFactory = null;
        }
    }

    private async Task<ServiceBusSender> GetOrOpenSenderLinkAsync(string entityName, CancellationToken cancellationToken)
    {
        if (_senderLinks.TryGetValue(entityName, out var senderLink) && !senderLink.IsClosed)
        {
            return senderLink;
        }

        await _senderLinksLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_senderLinks.TryGetValue(entityName, out senderLink) && !senderLink.IsClosed)
            {
                return senderLink;
            }

            var hasConnector = Connector.TryGetTarget(out var connector) && (connector != null);

            ObjectDisposedException.ThrowIf(!hasConnector, this);

            senderLink = connector!.OpenSenderLink(entityName);

            _senderLinks[entityName] = senderLink;

            return senderLink;
        }
        finally
        {
            _ = _senderLinksLock.Release();
        }
    }

    protected override async ValueTask OnDisposeAsync()
    {
        await _senderLinksLock.WaitAsync().ConfigureAwait(false);

        try
        {
            var hasConnector = Connector.TryGetTarget(out var connector);

            if (hasConnector && (connector != null))
            {
                foreach (var senderLink in _senderLinks.Values)
                {
                    await ExceptionProtector.ExecuteTaskAsync(connector.CloseSenderLinkAsync(senderLink)).ConfigureAwait(false);
                }
            }

            _senderLinks.Clear();
        }
        finally
        {
            _ = _senderLinksLock.Release();
        }

        _senderLinksLock.Dispose();
    }

    protected override void OnDispose()
    {
        _senderLinksLock.Wait();

        try
        {
            var hasConnector = Connector.TryGetTarget(out var connector);

            if (hasConnector && (connector != null))
            {
                foreach (var senderLink in _senderLinks.Values)
                {
                    ExceptionProtector.ExecuteAction(() => connector.CloseSenderLinkAsync(senderLink).GetAwaiter().GetResult());
                }
            }

            _senderLinks.Clear();
        }
        finally
        {
            _ = _senderLinksLock.Release();
        }

        _senderLinksLock.Dispose();
    }
}
