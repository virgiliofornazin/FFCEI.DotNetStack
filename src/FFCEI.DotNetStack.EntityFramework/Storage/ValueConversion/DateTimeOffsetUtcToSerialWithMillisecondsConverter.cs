using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeOffsetUtcToSerialWithMillisecondsConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<DateTimeOffset, long>(static to => To(to), static from => From(from), mappingHints)
{
    public DateTimeOffsetUtcToSerialWithMillisecondsConverter()
        : this(null)
    {
    }

    public static long To(DateTimeOffset to)
        => ValueConversions.DateTimeOffsetUtcToSerialWithMilliseconds(to);

    public static DateTimeOffset From(long from)
        => ValueConversions.DateTimeOffsetUtcFromSerialWithMilliseconds(from);
}
