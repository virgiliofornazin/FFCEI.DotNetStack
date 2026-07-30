using Apache.Qpid.Proton.Client;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;

namespace FFCEI.DotNetStack.MessageBroker.Qpid;

public sealed class QpidSender(string subject, WeakReference<QpidConnector> connector)
    : Sender(MessageBrokerProviderType.Qpid, subject)
{
    private readonly Dictionary<string, ISender> _senderLinks = [];
#pragma warning disable CA2213 // Disposable fields should be disposed
    private readonly SemaphoreSlim _senderLinksLock = new(1, 1);
#pragma warning restore CA2213 // Disposable fields should be disposed

    public WeakReference<QpidConnector> Connector { get; private set; } = connector;

    protected override async Task DoSendToAsync(MessageBrokerAddress address, MessageEnvelope messageEnvelope, CancellationToken cancellationToken = default)
    {
        var addressString = QpidConnector.ExtractQpidAddress(address);

        using var cancellationTokenSourceFactory = new CancellationTokenSourceFactory(cancellationToken);

        var token = cancellationTokenSourceFactory.Token;

        try
        {
            CancellationTokenSourceFactory = cancellationTokenSourceFactory;

            messageEnvelope.SerializeEnvelope(Serializer);

            var message = QpidConnector.MessageAttributesToQpidProtonMessage(messageEnvelope);

            var senderLink = await GetOrOpenSenderLinkAsync(addressString, token).ConfigureAwait(false);

            try
            {
                _ = await senderLink.SendAsync(message).WaitAsync(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                await _senderLinksLock.WaitAsync(token).ConfigureAwait(false);

                try
                {
                    _ = _senderLinks.Remove(addressString);
                }
                finally
                {
                    _ = _senderLinksLock.Release();
                }

                senderLink = await GetOrOpenSenderLinkAsync(addressString, token).ConfigureAwait(false);

                _ = await senderLink.SendAsync(message).WaitAsync(token).ConfigureAwait(false);
            }

            await Task.Yield();
        }
        finally
        {
            CancellationTokenSourceFactory = null;
        }
    }

    private async Task<ISender> GetOrOpenSenderLinkAsync(string addressString, CancellationToken cancellationToken)
    {
        if (_senderLinks.TryGetValue(addressString, out var senderLink))
        {
            return senderLink;
        }

        await _senderLinksLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_senderLinks.TryGetValue(addressString, out senderLink))
            {
                return senderLink;
            }

            var hasConnector = Connector.TryGetTarget(out var connector) && (connector != null);

            ObjectDisposedException.ThrowIf(!hasConnector, this);

            senderLink = await connector!.OpenSenderLinkAsync(addressString, cancellationToken).ConfigureAwait(false);

            _senderLinks[addressString] = senderLink;

            return senderLink;
        }
        finally
        {
            _ = _senderLinksLock.Release();
        }
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
                    _ = connector.CloseSenderLinkAsync(senderLink);
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
                    await connector.CloseSenderLinkAsync(senderLink).ConfigureAwait(false);
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
