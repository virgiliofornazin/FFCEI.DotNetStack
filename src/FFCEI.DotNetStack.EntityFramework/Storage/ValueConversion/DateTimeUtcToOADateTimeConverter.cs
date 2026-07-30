using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeUtcToOADateTimeConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<DateTime, double>(static to => To(to), static from => From(from), mappingHints)
{
    public DateTimeUtcToOADateTimeConverter()
        : this(null)
    {
    }

    public static double To(DateTime to)
    {
        var utcTo = ValueConversions.DateTimeAdjustToUtcIfNeeded(to);

        if (utcTo == DateTime.MinValue)
        {
            throw new ArgumentOutOfRangeException(nameof(to), "DateTime.MinValue cannot be represented as an OLE Automation date.");
        }

        return utcTo.ToOADate();
    }

    public static DateTime From(double from)
        => ValueConversions.DateTimeAdjustToUtcIfNeeded(DateTime.FromOADate(from));
}
