using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeToUtcConverter(ConverterMappingHints? mappingHints, DateTimeKind? overrideKind = null)
    : ValueConverter<DateTime, DateTime>(to => To(to, overrideKind), static from => From(from), mappingHints)
{
    public DateTimeToUtcConverter()
        : this(null)
    {
    }

    public static DateTime To(DateTime to, DateTimeKind? overrideKind = null)
        => ValueConversions.DateTimeAdjustToUtcIfNeededOr(to, overrideKind);

    public static DateTime From(DateTime from)
        => ValueConversions.DateTimeAdjustToUtcIfNeeded(from);
}
