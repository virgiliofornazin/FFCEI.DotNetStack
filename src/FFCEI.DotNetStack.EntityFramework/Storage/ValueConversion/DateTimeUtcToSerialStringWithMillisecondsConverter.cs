using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeUtcToSerialStringWithMillisecondsConverter(ConverterMappingHints? mappingHints, CultureInfo? cultureInfo = null)
    : ValueConverter<DateTime, string>(to => To(to, cultureInfo), from => From(from, cultureInfo), mappingHints)
{
    public DateTimeUtcToSerialStringWithMillisecondsConverter()
        : this(null)
    {
    }

    public static string To(DateTime to, CultureInfo? cultureInfo = null)
        => ValueConversions.DateTimeUtcToSerialWithMilliseconds(to).ToString(cultureInfo ?? CultureInfo.InvariantCulture);

    public static DateTime From(string from, CultureInfo? cultureInfo = null)
        => ValueConversions.DateTimeUtcFromSerialWithMilliseconds(long.Parse(from, cultureInfo ?? CultureInfo.InvariantCulture));
}
