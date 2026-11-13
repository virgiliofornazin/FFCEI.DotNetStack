using FFCEI.DotNetStack.Serialization.Converters.Json.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using decimal32 = System.Decimal;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

internal sealed class JsonStringToDecimal32ConverterFactory
    : JsonStringToNumberConverterFactory<decimal32>
{
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => new JsonStringToDecimal32Converter();
}
