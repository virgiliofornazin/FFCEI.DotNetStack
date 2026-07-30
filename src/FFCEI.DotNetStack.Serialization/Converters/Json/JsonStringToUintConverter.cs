using FFCEI.DotNetStack.Serialization.Converters.Json.Generic;
using System.Globalization;
using System.Text.Json;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

internal sealed class JsonStringToUintConverter
    : JsonStringToNumberConverter<uint>
{
    protected override void ReadValueFromNumber(ref Utf8JsonReader reader, out uint numberValue)
        => numberValue = reader.GetUInt32();

    protected override bool TryReadValueFromString(string stringValue, out uint numberValue)
        => uint.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out numberValue);

    public override void Write(Utf8JsonWriter writer, uint value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
