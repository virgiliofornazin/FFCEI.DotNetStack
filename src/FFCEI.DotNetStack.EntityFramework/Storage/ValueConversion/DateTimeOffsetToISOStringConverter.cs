using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeOffsetToISOStringConverter(ConverterMappingHints? mappingHints, CultureInfo? cultureInfo = null)
    : ValueConverter<DateTimeOffset, string>(to => To(to, cultureInfo), from => From(from, cultureInfo), mappingHints)
{
    public DateTimeOffsetToISOStringConverter()
        : this(null)
    {
    }

    public static string To(DateTimeOffset to, CultureInfo? cultureInfo = null)
        => to.ToString("O", cultureInfo ?? CultureInfo.InvariantCulture);

    public static DateTimeOffset From(string from, CultureInfo? cultureInfo = null)
        => DateTimeOffset.ParseExact(from, "O", cultureInfo ?? CultureInfo.InvariantCulture);
}
