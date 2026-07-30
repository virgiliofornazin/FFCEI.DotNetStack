using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeUtcToSerialWithMillisecondsConverter
    : ValueConverter<DateTime, long>
{
    public DateTimeUtcToSerialWithMillisecondsConverter()
        : this(null)
    {
    }

    public DateTimeUtcToSerialWithMillisecondsConverter(ConverterMappingHints? mappingHints)
        : base(static to => To(to), static from => From(from), mappingHints)
    {
    }

    public static long To(DateTime to)
        => ValueConversions.DateTimeUtcToSerialWithMilliseconds(to);

    public static DateTime From(long from)
        => ValueConversions.DateTimeUtcFromSerialWithMilliseconds(from);
}
