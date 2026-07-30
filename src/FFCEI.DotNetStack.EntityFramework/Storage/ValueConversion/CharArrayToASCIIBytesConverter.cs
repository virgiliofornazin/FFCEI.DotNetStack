using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class CharArrayToASCIIBytesConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<char[], byte[]>(static to => To(to), static from => From(from), mappingHints)
{
    private static readonly Lazy<Encoding> _restrictASCIIEncoding = new(() => Encoding.GetEncoding("us-ascii", new EncoderExceptionFallback(), new DecoderExceptionFallback()));

    public CharArrayToASCIIBytesConverter()
        : this(null)
    {
    }

    public static byte[] To(char[] to)
        => _restrictASCIIEncoding.Value.GetBytes(to);

    public static char[] From(byte[] from)
        => _restrictASCIIEncoding.Value.GetChars(from);
}
