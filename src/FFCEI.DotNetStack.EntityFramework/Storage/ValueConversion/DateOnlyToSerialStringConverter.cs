using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateOnlyToSerialStringConverter(ConverterMappingHints? mappingHints, CultureInfo? cultureInfo = null)
    : ValueConverter<DateOnly, string>(to => To(to, cultureInfo), from => From(from, cultureInfo), mappingHints)
{
    public DateOnlyToSerialStringConverter()
        : this(null)
    {
    }

    public static string To(DateOnly to, CultureInfo? cultureInfo = null)
        => ValueConversions.DateOnlyToSerial(to).ToString(cultureInfo ?? CultureInfo.InvariantCulture);

    public static DateOnly From(string from, CultureInfo? cultureInfo = null)
        => ValueConversions.DateOnlyFromSerial(int.Parse(from, cultureInfo ?? CultureInfo.InvariantCulture));
}
