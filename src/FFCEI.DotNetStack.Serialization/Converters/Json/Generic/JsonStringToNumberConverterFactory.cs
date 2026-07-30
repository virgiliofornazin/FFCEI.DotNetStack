using System.Numerics;
using System.Text.Json.Serialization;

namespace FFCEI.DotNetStack.Serialization.Converters.Json.Generic;

internal abstract class JsonStringToNumberConverterFactory<TNumber>
    : JsonConverterFactory where TNumber : INumber<TNumber>
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert == typeof(TNumber);
}
