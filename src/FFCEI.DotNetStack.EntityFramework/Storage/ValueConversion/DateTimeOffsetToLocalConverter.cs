using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeOffsetToLocalConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<DateTimeOffset, DateTimeOffset>(static to => To(to), static from => From(from), mappingHints)
{
    public DateTimeOffsetToLocalConverter()
        : this(null)
    {
    }

    public static DateTimeOffset To(DateTimeOffset to)
        => ValueConversions.DateTimeOffsetAdjustToLocalIfNeeded(to);

    public static DateTimeOffset From(DateTimeOffset from)
        => ValueConversions.DateTimeOffsetAdjustToLocalIfNeeded(from);
}
