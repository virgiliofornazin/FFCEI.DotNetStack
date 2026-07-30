using FFCEI.DotNetStack.Serialization.Converters.Json.Generic;
using System.Globalization;
using System.Text.Json;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

internal sealed class JsonStringToLongConverter
    : JsonStringToNumberConverter<long>
{
    protected override void ReadValueFromNumber(ref Utf8JsonReader reader, out long numberValue)
        => numberValue = reader.GetInt64();

    protected override bool TryReadValueFromString(string stringValue, out long numberValue)
        => long.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out numberValue);

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
