using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeOffsetToUnixMillisecondsConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<DateTimeOffset, long>(static to => To(to), static from => From(from), mappingHints)
{
    public DateTimeOffsetToUnixMillisecondsConverter()
        : this(null)
    {
    }

    public static long To(DateTimeOffset to)
        => ValueConversions.DateTimeOffsetAdjustToUtcIfNeeded(to).ToUnixTimeMilliseconds();

    public static DateTimeOffset From(long from)
        => ValueConversions.DateTimeOffsetAdjustToUtcIfNeeded(DateTimeOffset.FromUnixTimeMilliseconds(from));
}
