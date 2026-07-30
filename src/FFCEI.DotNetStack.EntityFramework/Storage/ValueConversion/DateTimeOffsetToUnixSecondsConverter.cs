using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeOffsetToUnixSecondsConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<DateTimeOffset, long>(static to => To(to), static from => From(from), mappingHints)
{
    public DateTimeOffsetToUnixSecondsConverter()
        : this(null)
    {
    }

    public static long To(DateTimeOffset to)
        => ValueConversions.DateTimeOffsetAdjustToUtcIfNeeded(to).ToUnixTimeSeconds();

    public static DateTimeOffset From(long from)
        => ValueConversions.DateTimeOffsetAdjustToUtcIfNeeded(DateTimeOffset.FromUnixTimeSeconds(from));
}
