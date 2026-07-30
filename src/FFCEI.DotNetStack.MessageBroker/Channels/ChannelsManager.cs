using FFCEI.DotNetStack.Framework;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace FFCEI.DotNetStack.MessageBroker.Channels;

public sealed class ChannelsManager
    : IDisposable, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, Channel<MessageEnvelope>> _channels = [];
    private bool _disposed;

    public static BoundedChannelOptions DefaultBoundedChannelOptions { get; private set; } = new(int.MaxValue)
    {
        SingleReader = false,
        SingleWriter = false
    };

    public static UnboundedChannelOptions DefaultUnboundedChannelOptions { get; private set; } = new()
    {
        SingleReader = false,
        SingleWriter = false
    };

    public IReadOnlyCollection<Channel<MessageEnvelope>>? Channels
        => _channels.Values.ToList().AsReadOnly();

    public Channel<MessageEnvelope> GetOrCreateBoundedChannel(string name, BoundedChannelOptions? options = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return _channels.GetOrAdd(name, _ => Channel.CreateBounded<MessageEnvelope>(options ?? DefaultBoundedChannelOptions));
    }

    public Channel<MessageEnvelope> GetOrCreateBoundedChannel(string name)
        => GetOrCreateBoundedChannel(name, DefaultBoundedChannelOptions);

    public Channel<MessageEnvelope> GetOrCreateUnboundedChannel(string name, UnboundedChannelOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return _channels.GetOrAdd(name, _ => Channel.CreateUnbounded<MessageEnvelope>(options ?? DefaultUnboundedChannelOptions));
    }

    public Channel<MessageEnvelope> GetOrCreateUnboundedChannel(string name)
        => GetOrCreateUnboundedChannel(name, DefaultUnboundedChannelOptions);

    public Channel<MessageEnvelope> GetOrCreateUnboundedPrioritizedChannel(string name, UnboundedPrioritizedChannelOptions<MessageEnvelope> options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return _channels.GetOrAdd(name, _ => Channel.CreateUnboundedPrioritized(options));
    }

    private static string ChannelName(string subject, MessageBrokerAddress address)
    {
        ArgumentNullException.ThrowIfNull(subject, nameof(subject));
        ArgumentNullException.ThrowIfNull(address, nameof(address));

        return $"{subject}:{address}";
    }

    public Channel<MessageEnvelope> Start(string subject, MessageBrokerAddress address, object? options = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var channelName = ChannelName(subject, address);

        if (_channels.TryGetValue(channelName, out var channel))
        {
            return channel;
        }

        if (options is BoundedChannelOptions boundedOptions)
        {
            channel = GetOrCreateBoundedChannel(channelName, boundedOptions);
        }
        else if (options is UnboundedChannelOptions unboundedOptions)
        {
            channel = GetOrCreateUnboundedChannel(channelName, unboundedOptions);
        }
        else if (options is UnboundedPrioritizedChannelOptions<MessageEnvelope> unboundedPrioritizedOptions)
        {
            channel = GetOrCreateUnboundedPrioritizedChannel(channelName, unboundedPrioritizedOptions);
        }
        else if (options is bool bounded)
        {
            channel = bounded ? GetOrCreateBoundedChannel(channelName) : GetOrCreateUnboundedChannel(channelName);
        }
        else
        {
            channel = GetOrCreateUnboundedChannel(channelName);
        }

        return channel;
    }

    public void Stop(string subject, MessageBrokerAddress address)
    {
        var channelName = ChannelName(subject, address);

        if (_channels.TryRemove(channelName, out var channel))
        {
            ExceptionProtector.ExecuteAction(() => channel.Writer.TryComplete());
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();

        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        var channels = _channels.Values;

        _channels.Clear();

        foreach (var channel in channels)
        {
            ExceptionProtector.ExecuteAction(() => channel.Writer.TryComplete());
        }

        _disposed = true;

        GC.SuppressFinalize(this);
    }

}
