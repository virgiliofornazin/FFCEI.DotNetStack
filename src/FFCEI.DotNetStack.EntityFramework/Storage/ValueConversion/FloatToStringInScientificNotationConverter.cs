using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class FloatToStringInScientificNotationConverter(ConverterMappingHints? mappingHints, CultureInfo? cultureInfo = null)
    : ValueConverter<float, string>(to => To(to, cultureInfo), from => From(from, cultureInfo), mappingHints)
{
    public FloatToStringInScientificNotationConverter()
        : this(null)
    {
    }

    public static string To(float to, CultureInfo? cultureInfo = null)
        => to.ToString("E9", cultureInfo ?? CultureInfo.InvariantCulture);

    public static float From(string from, CultureInfo? cultureInfo = null)
        => float.Parse(from, NumberStyles.Float, cultureInfo ?? CultureInfo.InvariantCulture);
}
