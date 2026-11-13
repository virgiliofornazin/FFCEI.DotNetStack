using FFCEI.DotNetStack.Serialization.Converters.Json.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using decimal64 = System.Decimal;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

internal sealed class JsonStringToDecimal64ConverterFactory
    : JsonStringToNumberConverterFactory<decimal64>
{
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => new JsonStringToDecimal64Converter();
}
