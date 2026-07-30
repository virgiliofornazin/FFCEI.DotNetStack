using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class TimeOnlyToSerialStringConverter(ConverterMappingHints? mappingHints, CultureInfo? cultureInfo = null)
    : ValueConverter<TimeOnly, string>(to => To(to, cultureInfo), from => From(from, cultureInfo), mappingHints)
{
    public TimeOnlyToSerialStringConverter()
        : this(null)
    {
    }

    public static string To(TimeOnly to, CultureInfo? cultureInfo = null)
        => ValueConversions.TimeOnlyToSerial(to).ToString(cultureInfo ?? CultureInfo.InvariantCulture);

    public static TimeOnly From(string from, CultureInfo? cultureInfo = null)
        => ValueConversions.TimeOnlyFromSerial(int.Parse(from, cultureInfo ?? CultureInfo.InvariantCulture));
}
