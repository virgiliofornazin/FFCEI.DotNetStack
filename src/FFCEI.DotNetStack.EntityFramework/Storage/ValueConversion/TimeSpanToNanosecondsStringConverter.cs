using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class TimeSpanToNanosecondsStringConverter(ConverterMappingHints? mappingHints, CultureInfo? cultureInfo = null)
    : ValueConverter<TimeSpan, string>(to => To(to, cultureInfo), from => From(from, cultureInfo), mappingHints)
{
    public TimeSpanToNanosecondsStringConverter()
        : this(null)
    {
    }

    public static string To(TimeSpan to, CultureInfo? cultureInfo = null)
        => ValueConversions.TimeSpanToNanoseconds(to).ToString(cultureInfo ?? CultureInfo.InvariantCulture);

    public static TimeSpan From(string from, CultureInfo? cultureInfo = null)
        => ValueConversions.TimeSpanFromNanoseconds(long.Parse(from, cultureInfo ?? CultureInfo.InvariantCulture));
}
