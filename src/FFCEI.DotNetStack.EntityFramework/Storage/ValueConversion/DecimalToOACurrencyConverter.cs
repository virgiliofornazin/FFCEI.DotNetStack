using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DecimalToOACurrencyConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<decimal, long>(static to => To(to), static from => From(from), mappingHints)
{
    public DecimalToOACurrencyConverter()
        : this(null)
    {
    }

    public static long To(decimal to)
        => decimal.ToOACurrency(to);

    public static decimal From(long from)
        => decimal.FromOACurrency(from);
}
