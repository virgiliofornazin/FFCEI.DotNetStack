using FFCEI.DotNetStack.Serialization.Converters.Json.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

internal sealed class JsonStringToIntConverterFactory
    : JsonStringToNumberConverterFactory<int>
{
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => new JsonStringToIntConverter();
}
