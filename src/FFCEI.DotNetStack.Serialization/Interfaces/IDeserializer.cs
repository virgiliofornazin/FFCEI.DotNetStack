using System.Buffers;

namespace FFCEI.DotNetStack.Serialization.Interfaces;

public interface IDeserializer
{
    SerializationAlgorithm SerializationAlgorithm { get; }

    object? DeserializeAs(Type type, Stream stream);
    object? DeserializeAs(Type type, ReadOnlyMemory<byte> buffer);
    object? DeserializeAs(Type type, ReadOnlySpan<byte> buffer);
    object? DeserializeAs(Type type, ReadOnlySequence<byte> buffer);
    object? DeserializeAs(Type type, byte[] buffer);
    IReadOnlyList<object>? DeserializeCollectionAs(Type type, Stream stream);
    IReadOnlyList<object>? DeserializeCollectionAs(Type type, ReadOnlyMemory<byte> buffer);
    IReadOnlyList<object>? DeserializeCollectionAs(Type type, ReadOnlySpan<byte> buffer);
    IReadOnlyList<object>? DeserializeCollectionAs(Type type, ReadOnlySequence<byte> buffer);
    IReadOnlyList<object>? DeserializeCollectionAs(Type type, byte[] buffer);
    TValue? Deserialize<TValue>(Stream stream);
    TValue? Deserialize<TValue>(ReadOnlyMemory<byte> buffer);
    TValue? Deserialize<TValue>(ReadOnlySpan<byte> buffer);
    TValue? Deserialize<TValue>(ReadOnlySequence<byte> buffer);
    TValue? Deserialize<TValue>(byte[] buffer);
    IReadOnlyList<TValue>? DeserializeCollection<TValue>(Stream stream);
    IReadOnlyList<TValue>? DeserializeCollection<TValue>(ReadOnlyMemory<byte> buffer);
    IReadOnlyList<TValue>? DeserializeCollection<TValue>(ReadOnlySpan<byte> buffer);
    IReadOnlyList<TValue>? DeserializeCollection<TValue>(ReadOnlySequence<byte> buffer);
    IReadOnlyList<TValue>? DeserializeCollection<TValue>(byte[] buffer);
}
