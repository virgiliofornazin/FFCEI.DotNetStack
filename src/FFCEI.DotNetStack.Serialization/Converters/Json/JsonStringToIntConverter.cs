using FFCEI.DotNetStack.Serialization.Converters.Json.Generic;
using System.Globalization;
using System.Text.Json;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

internal sealed class JsonStringToIntConverter
    : JsonStringToNumberConverter<int>
{
    protected override void ReadValueFromNumber(ref Utf8JsonReader reader, out int numberValue)
        => numberValue = reader.GetInt32();

    protected override bool TryReadValueFromString(string stringValue, out int numberValue)
        => int.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out numberValue);

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
