using FFCEI.DotNetStack.Serialization.Converters.Json.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using decimal128 = System.Decimal;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via Activator.CreateInstance in JsonStringToEnumConverterFactory.")]
internal sealed class JsonStringToDecimal128Converter
    : JsonStringToNumberConverter<decimal128>
{
    protected override void ReadValueFromNumber(ref Utf8JsonReader reader, out decimal128 numberValue)
        => numberValue = reader.GetDecimal();

    protected override bool TryReadValueFromString(string valueString, out decimal128 numberValue)
        => decimal128.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out numberValue);

    public override void Write(Utf8JsonWriter writer, decimal128 value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
