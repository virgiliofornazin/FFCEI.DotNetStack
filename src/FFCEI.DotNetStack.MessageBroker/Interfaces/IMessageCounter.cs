namespace FFCEI.DotNetStack.MessageBroker.Interfaces;

public interface IMessageCounter
{
    long MessageCount { get; }
    long ErrorCount { get; }
}
