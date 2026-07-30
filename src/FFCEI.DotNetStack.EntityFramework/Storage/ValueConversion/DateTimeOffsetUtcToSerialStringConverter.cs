using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeOffsetUtcToSerialStringConverter(ConverterMappingHints? mappingHints, CultureInfo? cultureInfo = null)
    : ValueConverter<DateTimeOffset, string>(to => To(to, cultureInfo), from => From(from, cultureInfo), mappingHints)
{
    public DateTimeOffsetUtcToSerialStringConverter()
        : this(null)
    {
    }

    public static string To(DateTimeOffset to, CultureInfo? cultureInfo = null)
        => ValueConversions.DateTimeOffsetUtcToSerial(to).ToString(cultureInfo ?? CultureInfo.InvariantCulture);

    public static DateTimeOffset From(string from, CultureInfo? cultureInfo = null)
        => ValueConversions.DateTimeOffsetUtcFromSerial(long.Parse(from, cultureInfo ?? CultureInfo.InvariantCulture));
}
