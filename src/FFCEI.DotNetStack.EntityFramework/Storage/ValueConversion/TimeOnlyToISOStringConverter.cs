using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class TimeOnlyToISOStringConverter(ConverterMappingHints? mappingHints, CultureInfo? cultureInfo = null)
    : ValueConverter<TimeOnly, string>(to => To(to, cultureInfo), from => From(from, cultureInfo), mappingHints)
{
    public TimeOnlyToISOStringConverter()
        : this(null)
    {
    }

    public static string To(TimeOnly to, CultureInfo? cultureInfo = null)
        => to.ToString("O", cultureInfo ?? CultureInfo.InvariantCulture);

    public static TimeOnly From(string from, CultureInfo? cultureInfo = null)
        => TimeOnly.ParseExact(from, "O", cultureInfo ?? CultureInfo.InvariantCulture);
}
