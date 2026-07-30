using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateOnlyToSerialConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<DateOnly, int>(static to => To(to), static from => From(from), mappingHints)
{
    public DateOnlyToSerialConverter()
        : this(null)
    {
    }

    public static int To(DateOnly to)
        => ValueConversions.DateOnlyToSerial(to);

    public static DateOnly From(int from)
        => ValueConversions.DateOnlyFromSerial(from);
}
