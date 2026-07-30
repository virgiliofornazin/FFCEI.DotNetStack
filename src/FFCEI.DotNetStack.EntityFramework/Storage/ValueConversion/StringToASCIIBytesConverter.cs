using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class StringToASCIIBytesConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<string, byte[]>(static to => To(to), static from => From(from), mappingHints)
{
    private static readonly Lazy<Encoding> _restrictASCIIEncoding = new(() => Encoding.GetEncoding("us-ascii", new EncoderExceptionFallback(), new DecoderExceptionFallback()));

    public StringToASCIIBytesConverter()
        : this(null)
    {
    }

    public static byte[] To(string to)
        => _restrictASCIIEncoding.Value.GetBytes(to);

    public static string From(byte[] from)
        => _restrictASCIIEncoding.Value.GetString(from);
}
