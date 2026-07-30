using FFCEI.DotNetStack.Serialization.Converters.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFCEI.DotNetStack.Serialization.Defaults;

public static class JsonDefaults
{
    public static JsonSerializerOptions OptimizedForMessaging => field ??= AddConvertersTo(new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    });

    public static JsonSerializerOptions OptimizedForScreen => field ??= AddConvertersTo(new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = false,
        PropertyNameCaseInsensitive = false
    });

    public static JsonSerializerOptions FormattedForScreen => field ??= AddConvertersTo(new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = true,
        PropertyNameCaseInsensitive = false
    });

    public static JsonSerializerOptions ForDeserialization => field ??= AddConvertersTo(new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = false,
        PropertyNameCaseInsensitive = false
    });

    public static JsonSerializerOptions CreateOptions()
    {
        var result = new JsonSerializerOptions();

        return AddConvertersTo(result);
    }

    private static JsonSerializerOptions AddConvertersTo(JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringToDecimalConverterFactory());
        options.Converters.Add(new JsonStringToDoubleConverterFactory());
        options.Converters.Add(new JsonStringToFloatConverterFactory());
        options.Converters.Add(new JsonStringToIntConverterFactory());
        options.Converters.Add(new JsonStringToUintConverterFactory());
        options.Converters.Add(new JsonStringToLongConverterFactory());
        options.Converters.Add(new JsonStringToUlongConverterFactory());

        return options;
    }
}
