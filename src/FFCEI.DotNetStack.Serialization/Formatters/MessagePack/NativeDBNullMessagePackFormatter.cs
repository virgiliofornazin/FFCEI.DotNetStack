using MessagePack;
using MessagePack.Formatters;

namespace FFCEI.DotNetStack.Serialization.Formatters.MessagePack;

public sealed class NativeDBNullMessagePackFormatter
    : IMessagePackFormatter<DBNull?>
{
    public static readonly NativeDBNullMessagePackFormatter Instance = new();

    private NativeDBNullMessagePackFormatter()
    {
    }

    public void Serialize(ref MessagePackWriter writer, DBNull? value, MessagePackSerializerOptions options)
        => writer.WriteNil();

    public DBNull Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        _ = reader.ReadNil();

        return DBNull.Value;
    }
}
