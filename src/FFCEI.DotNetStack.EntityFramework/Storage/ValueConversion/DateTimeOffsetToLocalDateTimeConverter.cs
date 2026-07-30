using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeOffsetToLocalDateTimeConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<DateTimeOffset, DateTime>(static to => To(to), static from => From(from), mappingHints)
{
    public DateTimeOffsetToLocalDateTimeConverter()
        : this(null)
    {
    }

    public static DateTime To(DateTimeOffset to)
        => ValueConversions.DateTimeOffsetAdjustToLocalIfNeeded(to).DateTime;

    public static DateTimeOffset From(DateTime from)
        => ValueConversions.DateTimeAdjustToLocalIfNeeded(from);
}
