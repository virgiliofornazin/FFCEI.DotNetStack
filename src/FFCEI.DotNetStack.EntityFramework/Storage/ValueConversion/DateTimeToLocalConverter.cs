using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeToLocalConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<DateTime, DateTime>(static to => To(to), static from => From(from), mappingHints)
{
    public DateTimeToLocalConverter()
        : this(null)
    {
    }

    public static DateTime To(DateTime to)
        => ValueConversions.DateTimeAdjustToLocalIfNeeded(to);

    public static DateTime From(DateTime from)
        => ValueConversions.DateTimeAdjustToLocalIfNeeded(from);
}
