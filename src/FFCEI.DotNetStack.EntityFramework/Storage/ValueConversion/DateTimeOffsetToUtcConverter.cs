using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeOffsetToUtcConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<DateTimeOffset, DateTimeOffset>(static to => To(to), static from => From(from), mappingHints)
{
    public DateTimeOffsetToUtcConverter()
        : this(null)
    {
    }

    public static DateTimeOffset To(DateTimeOffset to)
        => ValueConversions.DateTimeOffsetAdjustToUtcIfNeeded(to);

    public static DateTimeOffset From(DateTimeOffset from)
        => ValueConversions.DateTimeOffsetAdjustToUtcIfNeeded(from);
}
