using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DoubleToOACurrencyConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<double, long>(static to => To(to), static from => From(from), mappingHints)
{
    public DoubleToOACurrencyConverter()
        : this(null)
    {
    }

    public static long To(double to)
        => decimal.ToOACurrency((decimal)to);

    public static double From(long from)
        => (double)decimal.FromOACurrency(from);
}
