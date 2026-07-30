using FFCEI.DotNetStack.Serialization.Converters.Json.Generic;
using System.Globalization;
using System.Text.Json;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

internal sealed class JsonStringToFloatConverter
    : JsonStringToNumberConverter<float>
{
    protected override void ReadValueFromNumber(ref Utf8JsonReader reader, out float numberValue)
        => numberValue = reader.GetSingle();

    protected override bool TryReadValueFromString(string stringValue, out float numberValue)
        => float.TryParse(stringValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out numberValue);

    public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
