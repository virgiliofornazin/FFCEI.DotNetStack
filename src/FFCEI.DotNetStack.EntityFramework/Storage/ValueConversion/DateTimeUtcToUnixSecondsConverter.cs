using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeUtcToUnixSecondsConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<DateTime, long>(static to => To(to), static from => From(from), mappingHints)
{
    public DateTimeUtcToUnixSecondsConverter()
        : this(null)
    {
    }

    public static long To(DateTime to)
        => ValueConversions.DateTimeUtcToUnixSeconds(to);

    public static DateTime From(long from)
        => ValueConversions.DateTimeUtcFromUnixSeconds(from);
}
