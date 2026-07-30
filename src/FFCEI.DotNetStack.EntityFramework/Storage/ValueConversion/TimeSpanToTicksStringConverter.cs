using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class TimeSpanToTicksStringConverter(ConverterMappingHints? mappingHints, CultureInfo? cultureInfo = null)
    : ValueConverter<TimeSpan, string>(to => To(to, cultureInfo), from => From(from, cultureInfo), mappingHints)
{
    public TimeSpanToTicksStringConverter()
        : this(null)
    {
    }

    public static string To(TimeSpan to, CultureInfo? cultureInfo = null)
        => to.Ticks.ToString(cultureInfo ?? CultureInfo.InvariantCulture);

    public static TimeSpan From(string from, CultureInfo? cultureInfo = null)
        => TimeSpan.FromTicks(long.Parse(from, cultureInfo ?? CultureInfo.InvariantCulture));
}
