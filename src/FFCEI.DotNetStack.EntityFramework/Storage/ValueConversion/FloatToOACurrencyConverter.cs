using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class FloatToOACurrencyConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<float, long>(static to => To(to), static from => From(from), mappingHints)
{
    public FloatToOACurrencyConverter()
        : this(null)
    {
    }

    public static long To(float to)
        => decimal.ToOACurrency((decimal)to);

    public static float From(long from)
        => (float)decimal.FromOACurrency(from);
}
