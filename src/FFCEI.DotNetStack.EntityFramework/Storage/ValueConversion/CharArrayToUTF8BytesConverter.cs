using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class CharArrayToUTF8BytesConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<char[], byte[]>(static to => To(to), static from => From(from), mappingHints)
{
    public CharArrayToUTF8BytesConverter()
        : this(null)
    {
    }

    public static byte[] To(char[] to)
        => Encoding.UTF8.GetBytes(to);

    public static char[] From(byte[] from)
        => Encoding.UTF8.GetChars(from);
}
