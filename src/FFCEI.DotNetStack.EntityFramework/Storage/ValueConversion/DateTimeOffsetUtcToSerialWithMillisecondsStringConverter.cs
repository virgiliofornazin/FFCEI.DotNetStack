using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeOffsetUtcToSerialWithMillisecondsStringConverter(ConverterMappingHints? mappingHints, CultureInfo? cultureInfo = null)
    : ValueConverter<DateTimeOffset, string>(to => To(to, cultureInfo), from => From(from, cultureInfo), mappingHints)
{
    public DateTimeOffsetUtcToSerialWithMillisecondsStringConverter()
        : this(null)
    {
    }

    public static string To(DateTimeOffset to, CultureInfo? cultureInfo = null)
        => ValueConversions.DateTimeOffsetUtcToSerialWithMilliseconds(to).ToString(cultureInfo ?? CultureInfo.InvariantCulture);

    public static DateTimeOffset From(string from, CultureInfo? cultureInfo = null)
        => ValueConversions.DateTimeOffsetUtcFromSerialWithMilliseconds(long.Parse(from, cultureInfo ?? CultureInfo.InvariantCulture));
}
