using System.Collections;

namespace FFCEI.DotNetStack.Serialization.Interfaces;

public interface ISerializer
{
    SerializationAlgorithm SerializationAlgorithm { get; }

    void SerializeAs(Type type, object? value, Stream stream);
    void SerializeAs(Type type, object? value, ref byte[] buffer);
    void SerializeCollectionAs(Type type, IEnumerable? value, Stream stream);
    void SerializeCollectionAs(Type type, IEnumerable? value, ref byte[] buffer);
    void Serialize<TValue>(TValue? value, Stream stream);
    void Serialize<TValue>(TValue? value, ref byte[] buffer);
    void SerializeCollection<TValue>(IEnumerable<TValue>? value, ref byte[] buffer);
    void SerializeCollection<TValue>(IEnumerable<TValue>? value, Stream stream);
}
