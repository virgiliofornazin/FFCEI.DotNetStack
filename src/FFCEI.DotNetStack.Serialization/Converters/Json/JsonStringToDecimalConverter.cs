using FFCEI.DotNetStack.Serialization.Converters.Json.Generic;
using System.Globalization;
using System.Text.Json;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

internal sealed class JsonStringToDecimalConverter
    : JsonStringToNumberConverter<decimal>
{
    protected override void ReadValueFromNumber(ref Utf8JsonReader reader, out decimal numberValue)
        => numberValue = reader.GetDecimal();

    protected override bool TryReadValueFromString(string valueString, out decimal numberValue)
        => decimal.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out numberValue);

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
