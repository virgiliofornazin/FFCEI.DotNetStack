using FFCEI.DotNetStack.Serialization.Converters.Json.Generic;
using System.Globalization;
using System.Text.Json;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

internal sealed class JsonStringToDoubleConverter
    : JsonStringToNumberConverter<double>
{
    protected override void ReadValueFromNumber(ref Utf8JsonReader reader, out double numberValue)
        => numberValue = reader.GetDouble();

    protected override bool TryReadValueFromString(string stringValue, out double numberValue)
        => double.TryParse(stringValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out numberValue);

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
