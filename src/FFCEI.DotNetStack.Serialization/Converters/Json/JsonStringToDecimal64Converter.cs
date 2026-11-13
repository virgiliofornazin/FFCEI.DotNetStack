using FFCEI.DotNetStack.Serialization.Converters.Json.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using decimal64 = System.Decimal;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via Activator.CreateInstance in JsonStringToEnumConverterFactory.")]
internal sealed class JsonStringToDecimal64Converter
    : JsonStringToNumberConverter<decimal64>
{
    protected override void ReadValueFromNumber(ref Utf8JsonReader reader, out decimal64 numberValue)
        => numberValue = reader.GetDecimal();

    protected override bool TryReadValueFromString(string valueString, out decimal64 numberValue)
        => decimal64.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out numberValue);

    public override void Write(Utf8JsonWriter writer, decimal64 value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
