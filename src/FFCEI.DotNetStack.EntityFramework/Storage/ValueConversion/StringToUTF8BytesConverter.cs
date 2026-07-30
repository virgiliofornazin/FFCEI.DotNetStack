using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class StringToUTF8BytesConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<string, byte[]>(static to => To(to), static from => From(from), mappingHints)
{
    public StringToUTF8BytesConverter()
        : this(null)
    {
    }

    public static byte[] To(string to)
        => Encoding.UTF8.GetBytes(to);

    public static string From(byte[] from)
        => Encoding.UTF8.GetString(from);
}
