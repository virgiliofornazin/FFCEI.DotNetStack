using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeOffsetUtcToSerialConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<DateTimeOffset, long>(static to => To(to), static from => From(from), mappingHints)
{
    public DateTimeOffsetUtcToSerialConverter()
        : this(null)
    {
    }

    public static long To(DateTimeOffset to)
        => ValueConversions.DateTimeOffsetUtcToSerial(to);

    public static DateTimeOffset From(long from)
        => ValueConversions.DateTimeOffsetUtcFromSerial(from);
}
