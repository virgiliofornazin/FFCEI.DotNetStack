using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeUtcToUnixMillisecondsConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<DateTime, long>(static to => To(to), static from => From(from), mappingHints)
{
    public DateTimeUtcToUnixMillisecondsConverter()
        : this(null)
    {
    }

    public static long To(DateTime to)
        => ValueConversions.DateTimeUtcToUnixMilliseconds(to);

    public static DateTime From(long from)
        => ValueConversions.DateTimeUtcFromUnixMilliseconds(from);
}
