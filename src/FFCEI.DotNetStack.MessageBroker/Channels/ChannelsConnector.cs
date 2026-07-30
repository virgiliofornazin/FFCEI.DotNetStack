using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using FFCEI.DotNetStack.Serialization.Interfaces;
using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.MessageBroker.Channels;

public sealed class ChannelsConnector(ILogger<ChannelsConnector>? logger = null)
        : MessageBrokerConnector(MessageBrokerProviderType.Channels, logger)
{
    private bool _started;

    public ChannelsManager ChannelManager { get; private set; } = new();

    public override bool Started()
        => !Disposed && _started;

    private void ThrowIfAlreadyStarted()
    {
        if (_started)
        {
            throw new InvalidOperationException("ChannelConnector already started");
        }
    }

    public Task StartAsync()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ThrowIfAlreadyStarted();

        _started = true;

        MessageBrokerLog.ConnectorStarted(Logger, MessageBrokerProvider);

        return Task.CompletedTask;
    }

    public override Task StartAsync(Uri uri, CancellationToken cancellationToken = default)
        => StartAsync();

    public override Task StartAsync(string connectionString, CancellationToken cancellationToken = default)
        => StartAsync();

    public override Task StopAsync()
    {
        _started = false;

        return Task.CompletedTask;
    }

    public override async Task<ISender> GetSenderAsync(string subject, MessageBrokerAddress? defaultAddress = null, ISerializer? serializer = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (!_started)
        {
            throw new InvalidOperationException("ChannelConnectorFactory not started");
        }

        var sender = new ChannelsSender(subject, new WeakReference<ChannelsConnector>(this))
        {
            DefaultAddress = defaultAddress,
            Serializer = serializer
        };

        MessageBrokerLog.SenderCreated(Logger, MessageBrokerProvider, subject);

        return sender;
    }

    public override async Task<IReceiver> GetReceiverAsync(string subject, IDeserializer? deserializer = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (!_started)
        {
            throw new InvalidOperationException("ChannelConnectorFactory not started");
        }

        var receiver = new ChannelsReceiver(subject, new WeakReference<ChannelsConnector>(this))
        {
            Deserializer = deserializer
        };

        MessageBrokerLog.ReceiverCreated(Logger, MessageBrokerProvider, subject);

        return receiver;
    }

    protected override void OnDispose()
        => ChannelManager.DisposeObjectSafelyIfNeeded();

    protected override ValueTask OnDisposeAsync()
        => ChannelManager.DisposeSafelyIfNeededAsync();
}
