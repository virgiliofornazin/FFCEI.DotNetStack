using FFCEI.DotNetStack.Serialization.Converters.Json.Generic;
using System.Globalization;
using System.Text.Json;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

internal sealed class JsonStringToHalfConverter
    : JsonStringToNumberConverter<Half>
{
    protected override void ReadValueFromNumber(ref Utf8JsonReader reader, out Half numberValue)
        => numberValue = (Half)reader.GetSingle();

    protected override bool TryReadValueFromString(string stringValue, out Half numberValue)
        => Half.TryParse(stringValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out numberValue);

    public override void Write(Utf8JsonWriter writer, Half value, JsonSerializerOptions options)
        => writer.WriteNumberValue((float)value);
}
