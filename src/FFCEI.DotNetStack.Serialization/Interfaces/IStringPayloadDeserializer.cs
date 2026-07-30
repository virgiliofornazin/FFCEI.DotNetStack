namespace FFCEI.DotNetStack.Serialization.Interfaces;

public interface IStringPayloadDeserializer
{
    object? DeserializeAs(Type type, string buffer);
    IReadOnlyList<object>? DeserializeCollectionAs(Type type, string buffer);
    TValue? Deserialize<TValue>(string buffer);
    IReadOnlyList<TValue>? DeserializeCollection<TValue>(string buffer);
}
