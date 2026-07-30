using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeToISOStringConverter(ConverterMappingHints? mappingHints, CultureInfo? cultureInfo = null)
    : ValueConverter<DateTime, string>(to => To(to, cultureInfo), from => From(from, cultureInfo), mappingHints)
{
    public DateTimeToISOStringConverter()
        : this(null)
    {
    }

    public static string To(DateTime to, CultureInfo? cultureInfo = null)
        => to.ToString("O", cultureInfo ?? CultureInfo.InvariantCulture);

    public static DateTime From(string from, CultureInfo? cultureInfo = null)
    {
        ArgumentNullException.ThrowIfNull(from, nameof(from));

        var result = DateTime.ParseExact(from, "O", cultureInfo ?? CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        return ((from[from.Length - 1] == 'Z') && result.Kind == DateTimeKind.Local) ? result.ToUniversalTime() : result;
    }
}
