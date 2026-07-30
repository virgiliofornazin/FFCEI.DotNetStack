using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeUtcToSerialConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<DateTime, long>(static to => To(to), static from => From(from), mappingHints)
{
    public DateTimeUtcToSerialConverter()
        : this(null)
    {
    }

    public static long To(DateTime to)
        => ValueConversions.DateTimeUtcToSerial(to);

    public static DateTime From(long from)
        => ValueConversions.DateTimeUtcFromSerial(from);
}
