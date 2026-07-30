using System.Diagnostics;

namespace FFCEI.DotNetStack.MessageBroker;

[DebuggerDisplay("MessageBrokerAddress: {(IsCustomAddressType ? CustomAddressType : Type.ToString()),nq} => {Value,nq} [Valid: {IsValidAddress}]")]
public sealed class MessageBrokerAddress
    : IEquatable<MessageBrokerAddress>, IComparable<MessageBrokerAddress>
{
    public required MessageBrokerAddressType Type
    {
        get;
        set
        {
            field = value;

            if (value != MessageBrokerAddressType.CustomExchange)
            {
                CustomExchange = null;
            }
        }
    }

    public string? CustomExchange
    {
        get;
        set
        {
            field = value;

            if (!string.IsNullOrWhiteSpace(value) && (Type != MessageBrokerAddressType.CustomExchange))
            {
                Type = MessageBrokerAddressType.CustomExchange;
            }
        }
    }

    public required string Value { get; set; }

    public bool IsCustomAddressType
        => Type == MessageBrokerAddressType.CustomExchange && !string.IsNullOrEmpty(CustomExchange);

    public bool IsValidAddress
        => ((Type != MessageBrokerAddressType.CustomExchange) || !string.IsNullOrWhiteSpace(CustomExchange)) && !string.IsNullOrWhiteSpace(Value);

    public override string ToString()
        => $"{(IsCustomAddressType ? CustomExchange : Type.ToString())}.{Value}";

    public bool Equals(MessageBrokerAddress? other)
        => other is not null && (Type == other.Type) && (CustomExchange == other.CustomExchange) && (Value == other.Value);

    public override bool Equals(object? obj)
        => Equals(obj as MessageBrokerAddress);

    public override int GetHashCode()
        => HashCode.Combine(Type, CustomExchange, Value);

    public int CompareTo(MessageBrokerAddress? other)
    {
        if (other is null)
        {
            return 1;
        }

        var result = Type.CompareTo(other.Type);

        if (result != 0)
        {
            return result;
        }

        result = string.CompareOrdinal(CustomExchange, other.CustomExchange);

        if (result != 0)
        {
            return result;
        }

        return string.CompareOrdinal(Value, other.Value);
    }

    public static bool operator ==(MessageBrokerAddress? left, MessageBrokerAddress? right)
        => left is null ? right is null : left.Equals(right);

    public static bool operator !=(MessageBrokerAddress? left, MessageBrokerAddress? right)
        => !(left == right);

    public static bool operator <(MessageBrokerAddress? left, MessageBrokerAddress? right)
        => left is null ? right is not null : left.CompareTo(right) < 0;

    public static bool operator <=(MessageBrokerAddress? left, MessageBrokerAddress? right)
        => left is null || left.CompareTo(right) <= 0;

    public static bool operator >(MessageBrokerAddress? left, MessageBrokerAddress? right)
        => left is not null && left.CompareTo(right) > 0;

    public static bool operator >=(MessageBrokerAddress? left, MessageBrokerAddress? right)
        => left is null ? right is null : left.CompareTo(right) >= 0;

    public static MessageBrokerAddress Direct(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        var result = new MessageBrokerAddress()
        {
            Type = MessageBrokerAddressType.Direct,
            Value = value
        };

        return result;
    }

    public static MessageBrokerAddress Topic(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        var result = new MessageBrokerAddress()
        {
            Type = MessageBrokerAddressType.Topic,
            Value = value
        };

        return result;
    }

    public static MessageBrokerAddress Queue(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        var result = new MessageBrokerAddress()
        {
            Type = MessageBrokerAddressType.Queue,
            Value = value
        };

        return result;
    }

    public static MessageBrokerAddress Fanout(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        var result = new MessageBrokerAddress()
        {
            Type = MessageBrokerAddressType.Fanout,
            Value = value
        };

        return result;
    }

    public static MessageBrokerAddress Headers(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        var result = new MessageBrokerAddress()
        {
            Type = MessageBrokerAddressType.Headers,
            Value = value
        };

        return result;
    }

    public static MessageBrokerAddress Custom(string customAddressType, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customAddressType, nameof(customAddressType));
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        var result = new MessageBrokerAddress()
        {
            Type = MessageBrokerAddressType.CustomExchange,
            CustomExchange = customAddressType,
            Value = value
        };

        return result;
    }
}
