namespace FFCEI.DotNetStack.MessageBroker;

public enum MessageBrokerAddressType
{
    CustomExchange = 0,
    Direct = 1,
    Topic = 2,
    Queue = 3,
    Headers = 4,
    Fanout = 5
}
