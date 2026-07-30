using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DoubleToStringInScientificNotationConverter(ConverterMappingHints? mappingHints, CultureInfo? cultureInfo = null)
    : ValueConverter<double, string>(to => To(to, cultureInfo), from => From(from, cultureInfo), mappingHints)
{
    public DoubleToStringInScientificNotationConverter()
        : this(null)
    {
    }

    public static string To(double to, CultureInfo? cultureInfo = null)
        => to.ToString("E17", cultureInfo ?? CultureInfo.InvariantCulture);

    public static double From(string from, CultureInfo? cultureInfo = null)
        => double.Parse(from, NumberStyles.Float, cultureInfo ?? CultureInfo.InvariantCulture);
}
