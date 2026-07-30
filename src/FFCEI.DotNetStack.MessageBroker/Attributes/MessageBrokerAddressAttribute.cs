namespace FFCEI.DotNetStack.MessageBroker.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class MessageBrokerAddressAttribute
    : Attribute
{
    public MessageBrokerAddress Address { get; }
    public MessageBrokerAddressType Type => Address.Type;
    public string Value => Address.Value;
    public string? CustomAddressType => Address.CustomExchange;

    public MessageBrokerAddressAttribute(MessageBrokerAddress address)
    {
        ArgumentNullException.ThrowIfNull(address, nameof(address));

        Address = address;
    }

    public MessageBrokerAddressAttribute(MessageBrokerAddressType type, string value, string? customAddressType = null)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        Address = new()
        {
            Type = type,
            CustomExchange = customAddressType,
            Value = value
        };
    }

    public MessageBrokerAddressAttribute(string customAddressType, string value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        ArgumentNullException.ThrowIfNull(customAddressType, nameof(customAddressType));

        Address = new()
        {
            Type = MessageBrokerAddressType.CustomExchange,
            CustomExchange = customAddressType,
            Value = value
        };
    }
}
