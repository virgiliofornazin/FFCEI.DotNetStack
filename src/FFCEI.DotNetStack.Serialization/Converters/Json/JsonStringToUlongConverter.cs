using FFCEI.DotNetStack.Serialization.Converters.Json.Generic;
using System.Globalization;
using System.Text.Json;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

internal sealed class JsonStringToUlongConverter
    : JsonStringToNumberConverter<ulong>
{
    protected override void ReadValueFromNumber(ref Utf8JsonReader reader, out ulong numberValue)
        => numberValue = reader.GetUInt64();

    protected override bool TryReadValueFromString(string stringValue, out ulong numberValue)
        => ulong.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out numberValue);

    public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
