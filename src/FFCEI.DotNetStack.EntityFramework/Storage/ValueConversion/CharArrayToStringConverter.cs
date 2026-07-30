using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class CharArrayToStringConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<char[], string>(static to => To(to), static from => From(from), mappingHints)
{
    public CharArrayToStringConverter()
        : this(null)
    {
    }

    public static string To(char[] to)
        => to != null ? new string(to) : null!;

    public static char[] From(string from)
        => from?.ToCharArray() ?? null!;
}
