namespace FFCEI.DotNetStack.MessageBroker.Interfaces;

public interface IMessageBrokerProvider
{
    MessageBrokerProviderType MessageBrokerProvider { get; }
    string Subject { get; }
}
