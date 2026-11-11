using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFCEI.DotNetStack.Serialization.Converters.Json;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via Activator.CreateInstance in JsonStringToEnumConverterFactory.")]
internal sealed class JsonStringToEnumConverter<TEnum>
    : JsonConverter<TEnum> where TEnum : struct, Enum
{
    private static void ReadValueFromEnum(ref Utf8JsonReader reader, out TEnum enumValue)
    {
        var value = "value";

        if (reader.TryGetUInt64(out var ulongValue))
        {
            value = ulongValue.ToString(CultureInfo.InvariantCulture);

            var objectValue = Enum.ToObject(typeof(TEnum), ulongValue);

            if (objectValue is TEnum convertedValue)
            {
                enumValue = convertedValue;

                return;
            }
        }

        if (reader.TryGetInt64(out var longValue))
        {
            value = ulongValue.ToString(CultureInfo.InvariantCulture);

            var objectValue = Enum.ToObject(typeof(TEnum), longValue);

            if (objectValue is TEnum convertedValue)
            {
                enumValue = convertedValue;

                return;
            }
        }

        throw new JsonException($"Unable to convert number {value} to \"{typeof(TEnum)}\".");
    }

    private static void TryReadValueFromString(ref Utf8JsonReader reader, out TEnum enumValue)
    {
        var valueParse = (reader.GetString() ?? string.Empty).TrimStart().TrimEnd();

        if (Enum.TryParse(valueParse, true, out enumValue) ||
            Enum.TryParse(valueParse, false, out enumValue))
        {
            return;
        }

        throw new JsonException($"Unable to convert \"{valueParse}\" to \"{typeof(TEnum)}\".");
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            ReadValueFromEnum(ref reader, out var enumValue);

            return enumValue;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            TryReadValueFromString(ref reader, out var enumValue);

            return enumValue;
        }

        throw new JsonException($"Unable to convert json value to \"{typeof(TEnum)}\".");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        => writer.WriteNumberValue((long)((object)value));
}
