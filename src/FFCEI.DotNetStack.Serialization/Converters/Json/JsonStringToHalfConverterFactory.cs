using FFCEI.DotNetStack.Serialization.Converters.Json.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

internal sealed class JsonStringToHalfConverterFactory
    : JsonStringToNumberConverterFactory<Half>
{
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => new JsonStringToHalfConverter();
}
