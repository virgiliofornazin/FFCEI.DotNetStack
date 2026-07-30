using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeOffsetUtcToOADateTimeOffsetConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<DateTimeOffset, double>(static to => To(to), static from => From(from), mappingHints)
{
    public DateTimeOffsetUtcToOADateTimeOffsetConverter()
        : this(null)
    {
    }

    public static double To(DateTimeOffset to)
    {
        var utcTo = to.UtcDateTime;

        if (utcTo == DateTime.MinValue)
        {
            throw new ArgumentOutOfRangeException(nameof(to), "DateTimeOffset.UtcDateTime.MinValue cannot be represented as an OLE Automation date.");
        }

        return utcTo.ToOADate();
    }

    public static DateTimeOffset From(double from)
        => ValueConversions.DateTimeAdjustToUtcIfNeeded(DateTime.FromOADate(from));
}
