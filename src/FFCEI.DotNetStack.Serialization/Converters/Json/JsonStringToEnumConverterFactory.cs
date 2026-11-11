using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

internal sealed class JsonStringToEnumConverterFactory
    : JsonConverterFactory
{
    private static readonly Type EnumConverterType = typeof(JsonStringToEnumConverter<>);

    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsEnum;

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => Activator.CreateInstance(EnumConverterType.MakeGenericType([typeToConvert])) as JsonConverter;
}
