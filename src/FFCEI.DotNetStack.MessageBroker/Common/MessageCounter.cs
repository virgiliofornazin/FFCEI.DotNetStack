using FFCEI.DotNetStack.MessageBroker.Interfaces;

namespace FFCEI.DotNetStack.MessageBroker.Common;

public abstract class MessageCounter
    : IMessageCounter
{
    private long _messageCount;
    private long _errorCount;

    public long MessageCount => _messageCount;
    public long ErrorCount => _errorCount;

    protected void IncrementMessageCount()
    {
        _ = Interlocked.Increment(ref _messageCount);
    }

    protected void IncrementErrorCount()
    {
        _ = Interlocked.Increment(ref _errorCount);
    }
}
