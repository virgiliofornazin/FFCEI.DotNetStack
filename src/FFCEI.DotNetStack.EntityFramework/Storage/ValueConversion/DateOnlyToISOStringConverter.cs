using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateOnlyToISOStringConverter(ConverterMappingHints? mappingHints, CultureInfo? cultureInfo = null)
    : ValueConverter<DateOnly, string>(to => To(to, cultureInfo), from => From(from, cultureInfo), mappingHints)
{
    public DateOnlyToISOStringConverter()
        : this(null)
    {
    }

    public static string To(DateOnly to, CultureInfo? cultureInfo = null)
        => to.ToString("O", cultureInfo ?? CultureInfo.InvariantCulture);

    public static DateOnly From(string from, CultureInfo? cultureInfo = null)
        => DateOnly.ParseExact(from, "O", cultureInfo ?? CultureInfo.InvariantCulture);
}
