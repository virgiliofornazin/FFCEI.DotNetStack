using FFCEI.DotNetStack.Serialization.Converters.Json.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using decimal32 = System.Decimal;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via Activator.CreateInstance in JsonStringToEnumConverterFactory.")]
internal sealed class JsonStringToDecimal32Converter
    : JsonStringToNumberConverter<decimal32>
{
    protected override void ReadValueFromNumber(ref Utf8JsonReader reader, out decimal32 numberValue)
        => numberValue = reader.GetDecimal();

    protected override bool TryReadValueFromString(string valueString, out decimal32 numberValue)
        => decimal32.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out numberValue);

    public override void Write(Utf8JsonWriter writer, decimal32 value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
