using FFCEI.DotNetStack.Serialization.Interfaces;
using System.Collections;

namespace FFCEI.DotNetStack.Serialization.Common;

public abstract class Serializer
    : ISerializer
{
    public abstract SerializationAlgorithm SerializationAlgorithm { get; }

    public abstract void SerializeAs(Type type, object? value, Stream stream);

    public abstract void SerializeAs(Type type, object? value, ref byte[] buffer);

    public abstract void SerializeCollectionAs(Type type, IEnumerable? value, Stream stream);

    public abstract void SerializeCollectionAs(Type type, IEnumerable? value, ref byte[] buffer);

    public virtual void Serialize<TValue>(TValue? value, Stream stream)
       => SerializeAs(value?.GetType() ?? typeof(TValue), value, stream);

    public virtual void Serialize<TValue>(TValue? value, ref byte[] buffer)
        => SerializeAs(value?.GetType() ?? typeof(TValue), value, ref buffer);

    public virtual void SerializeCollection<TValue>(IEnumerable<TValue>? value, ref byte[] buffer)
        => SerializeAs(value?.FirstOrDefault()?.GetType() ?? typeof(TValue), value, ref buffer);

    public virtual void SerializeCollection<TValue>(IEnumerable<TValue>? value, Stream stream)
       => SerializeAs(value?.FirstOrDefault()?.GetType() ?? typeof(TValue), value, stream);
}
