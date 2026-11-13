using FFCEI.DotNetStack.Serialization.Converters.Json.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using decimal128 = System.Decimal;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

internal sealed class JsonStringToDecimal128ConverterFactory
    : JsonStringToNumberConverterFactory<decimal128>
{
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => new JsonStringToDecimal128Converter();
}
