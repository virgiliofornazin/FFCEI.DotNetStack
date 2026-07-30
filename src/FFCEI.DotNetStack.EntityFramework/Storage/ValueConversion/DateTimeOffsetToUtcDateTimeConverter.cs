using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeOffsetToUtcDateTimeConverter(ConverterMappingHints? mappingHints, DateTimeKind? overrideKind = null)
    : ValueConverter<DateTimeOffset, DateTime>(to => To(to, overrideKind), from => From(from, overrideKind), mappingHints)
{
    public DateTimeOffsetToUtcDateTimeConverter()
        : this(null)
    {
    }

    public static DateTime To(DateTimeOffset to, DateTimeKind? overrideKind = null)
        => DateTime.SpecifyKind(ValueConversions.DateTimeOffsetAdjustToUtcIfNeeded(to).UtcDateTime, overrideKind ?? DateTimeKind.Utc);

    public static DateTimeOffset From(DateTime from, DateTimeKind? overrideKind = null)
        => ValueConversions.DateTimeAdjustToUtcIfNeededOr(from, overrideKind == DateTimeKind.Unspecified ? null : overrideKind);
}
