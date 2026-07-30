using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFCEI.DotNetStack.Serialization.Converters.Json.Generic;

internal abstract class JsonStringToNumberConverter<TNumber>
    : JsonConverter<TNumber> where TNumber : INumber<TNumber>
{
    protected abstract void ReadValueFromNumber(ref Utf8JsonReader reader, out TNumber numberValue);
    protected abstract bool TryReadValueFromString(string valueString, out TNumber numberValue);

    public override TNumber Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            ReadValueFromNumber(ref reader, out var numberValue);

            return numberValue;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var valueString = reader.GetString() ?? string.Empty;

            if (TryReadValueFromString(valueString, out var numberValue))
            {
                return numberValue;
            }

            throw new JsonException($"Unable to convert \"{valueString}\" to \"{typeof(TNumber)}\".");
        }

        throw new JsonException($"Unable to convert to \"{typeof(TNumber)}\".");
    }
}
