namespace FFCEI.DotNetStack.MessageBroker.Interfaces;

public interface IConfigurableReceiver
    : IReceiver
{
    Task ConfigureAsync(MessageBrokerAddress address, CancellationToken cancellationToken = default);
}
