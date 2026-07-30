using FFCEI.DotNetStack.Serialization.Interfaces;
using System.Collections;
using System.Text;

namespace FFCEI.DotNetStack.Serialization;

public static class ISerializerExtensionMethods
{
    public static byte[]? SerializeToBytesAs(this ISerializer serializer, Type type, object? value)
    {
        ArgumentNullException.ThrowIfNull(serializer, nameof(serializer));

        if (value == null)
        {
            return null;
        }

        byte[] buffer = null!;

        serializer.SerializeAs(type, value, ref buffer);

        return buffer;
    }

    public static byte[]? SerializeToBytes<TValue>(this ISerializer serializer, TValue? value)
    {
        ArgumentNullException.ThrowIfNull(serializer, nameof(serializer));

        if (value == null)
        {
            return null;
        }

        byte[] buffer = null!;

        serializer.Serialize(value, ref buffer);

        return buffer;
    }

    public static byte[]? SerializeCollectionToBytesAs(this ISerializer serializer, Type type, IEnumerable? value)
    {
        ArgumentNullException.ThrowIfNull(serializer, nameof(serializer));

        if (value == null)
        {
            return null;
        }

        byte[] buffer = null!;

        serializer.SerializeCollectionAs(type, value, ref buffer);

        return buffer;
    }

    public static byte[]? SerializeCollectionToBytes<TValue>(this ISerializer serializer, IEnumerable<TValue>? value)
    {
        ArgumentNullException.ThrowIfNull(serializer, nameof(serializer));

        if (value == null)
        {
            return null;
        }

        byte[] buffer = null!;

        serializer.SerializeCollection(value, ref buffer);

        return buffer;
    }

    public static string? SerializedToStringAs(this ISerializer serializer, Type type, object? value)
    {
        ArgumentNullException.ThrowIfNull(serializer, nameof(serializer));

        if (value == null)
        {
            return null;
        }

        var serialized = SerializeToBytesAs(serializer, type, value);

        if (serialized == null)
        {
            return null;
        }

        return serializer is IStringPayloadSerializer ? Encoding.UTF8.GetString(serialized) : Convert.ToBase64String(serialized);
    }

    public static string? SerializedToString<TValue>(this ISerializer serializer, TValue? value)
    {
        ArgumentNullException.ThrowIfNull(serializer, nameof(serializer));

        if (value == null)
        {
            return null;
        }

        var serialized = SerializeToBytes(serializer, value);

        if (serialized == null)
        {
            return null;
        }

        return serializer is IStringPayloadSerializer ? Encoding.UTF8.GetString(serialized) : Convert.ToBase64String(serialized);
    }

    public static string? SerializedCollectionToStringAs(this ISerializer serializer, Type type, IEnumerable? value)
    {
        ArgumentNullException.ThrowIfNull(serializer, nameof(serializer));

        if (value == null)
        {
            return null;
        }

        var serialized = SerializeCollectionToBytesAs(serializer, type, value);

        if (serialized == null)
        {
            return null;
        }

        return serializer is IStringPayloadSerializer ? Encoding.UTF8.GetString(serialized) : Convert.ToBase64String(serialized);
    }

    public static string? SerializedCollectionToString<TValue>(this ISerializer serializer, IEnumerable<TValue>? value)
    {
        ArgumentNullException.ThrowIfNull(serializer, nameof(serializer));

        if (value == null)
        {
            return null;
        }

        var serialized = SerializeCollectionToBytes(serializer, value);

        if (serialized == null)
        {
            return null;
        }

        return serializer is IStringPayloadSerializer ? Encoding.UTF8.GetString(serialized) : Convert.ToBase64String(serialized);
    }
}
