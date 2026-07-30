using FFCEI.DotNetStack.Serialization.Interfaces;

namespace FFCEI.DotNetStack.Serialization.Common;

public abstract class StringPayloadDeserializer
    : Deserializer, IStringPayloadDeserializer
{
    public abstract object? DeserializeAs(Type type, string buffer);

    public virtual IReadOnlyList<object>? DeserializeCollectionAs(Type type, string buffer)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        return DeserializeAs(type.MakeArrayType(), buffer) as IReadOnlyList<object>;
    }

    public virtual TValue? Deserialize<TValue>(string buffer)
        => CheckResult<TValue>(DeserializeAs(typeof(TValue), buffer));

    public virtual IReadOnlyList<TValue>? DeserializeCollection<TValue>(string buffer)
        => CheckCollectionResult<TValue>(DeserializeCollectionAs(typeof(TValue), buffer));
}
