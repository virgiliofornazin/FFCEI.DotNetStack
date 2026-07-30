using FFCEI.DotNetStack.Serialization.Interfaces;
using System.Buffers;

namespace FFCEI.DotNetStack.Serialization.Common;

public abstract class Deserializer
    : IDeserializer
{
    public abstract SerializationAlgorithm SerializationAlgorithm { get; }

    internal static TValue? CheckResult<TValue>(object? result)
    {
        if ((result == null) || (result is not TValue value))
        {
            throw new InvalidCastException($"Cannot cast result type '{result?.GetType()}' to '{typeof(TValue)}'");
        }

        return value;
    }

    internal static IReadOnlyList<TValue>? CheckCollectionResult<TValue>(IReadOnlyList<object>? result)
    {
        if (result != null)
        {
            foreach (var item in result)
            {
                _ = CheckResult<TValue>(item);
            }
        }

        return result as IReadOnlyList<TValue>;
    }

    public abstract object? DeserializeAs(Type type, Stream stream);

    public abstract object? DeserializeAs(Type type, ReadOnlyMemory<byte> buffer);

    public abstract object? DeserializeAs(Type type, ReadOnlySpan<byte> buffer);

    public abstract object? DeserializeAs(Type type, ReadOnlySequence<byte> buffer);

    public abstract object? DeserializeAs(Type type, byte[] buffer);

    public virtual IReadOnlyList<object>? DeserializeCollectionAs(Type type, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        return DeserializeAs(type.MakeArrayType(), stream) as IReadOnlyList<object>;
    }

    public virtual IReadOnlyList<object>? DeserializeCollectionAs(Type type, ReadOnlyMemory<byte> buffer)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        return DeserializeAs(type.MakeArrayType(), buffer) as IReadOnlyList<object>;
    }

    public virtual IReadOnlyList<object>? DeserializeCollectionAs(Type type, ReadOnlySpan<byte> buffer)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        return DeserializeAs(type.MakeArrayType(), buffer) as IReadOnlyList<object>;
    }

    public virtual IReadOnlyList<object>? DeserializeCollectionAs(Type type, ReadOnlySequence<byte> buffer)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        return DeserializeAs(type.MakeArrayType(), buffer) as IReadOnlyList<object>;
    }

    public virtual IReadOnlyList<object>? DeserializeCollectionAs(Type type, byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        return DeserializeAs(type.MakeArrayType(), buffer) as IReadOnlyList<object>;
    }

    public virtual TValue? Deserialize<TValue>(Stream stream)
        => CheckResult<TValue>(DeserializeAs(typeof(TValue), stream));

    public virtual TValue? Deserialize<TValue>(ReadOnlyMemory<byte> buffer)
        => CheckResult<TValue>(DeserializeAs(typeof(TValue), buffer));

    public virtual TValue? Deserialize<TValue>(ReadOnlySpan<byte> buffer)
        => CheckResult<TValue>(DeserializeAs(typeof(TValue), buffer));

    public virtual TValue? Deserialize<TValue>(ReadOnlySequence<byte> buffer)
        => CheckResult<TValue>(DeserializeAs(typeof(TValue), buffer));

    public virtual TValue? Deserialize<TValue>(byte[] buffer)
        => CheckResult<TValue>(DeserializeAs(typeof(TValue), buffer));

    public virtual IReadOnlyList<TValue>? DeserializeCollection<TValue>(Stream stream)
        => CheckCollectionResult<TValue>(DeserializeCollectionAs(typeof(TValue), stream));

    public virtual IReadOnlyList<TValue>? DeserializeCollection<TValue>(ReadOnlyMemory<byte> buffer)
        => CheckCollectionResult<TValue>(DeserializeCollectionAs(typeof(TValue), buffer));

    public virtual IReadOnlyList<TValue>? DeserializeCollection<TValue>(ReadOnlySpan<byte> buffer)
        => CheckCollectionResult<TValue>(DeserializeCollectionAs(typeof(TValue), buffer));

    public virtual IReadOnlyList<TValue>? DeserializeCollection<TValue>(ReadOnlySequence<byte> buffer)
        => CheckCollectionResult<TValue>(DeserializeCollectionAs(typeof(TValue), buffer));

    public virtual IReadOnlyList<TValue>? DeserializeCollection<TValue>(byte[] buffer)
        => CheckCollectionResult<TValue>(DeserializeCollectionAs(typeof(TValue), buffer));
}
