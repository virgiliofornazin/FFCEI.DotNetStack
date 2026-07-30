using FFCEI.DotNetStack.Framework;
using System.Text;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.TestModels;

public static class FullMappingExtensionMethods
{
#pragma warning disable CA5394 // Do not use insecure randomness
    public static int NextRandom()
        => Random.Shared.Next();

    public static double NextDoubleRandom()
        => Random.Shared.NextDouble();
#pragma warning restore CA5394 // Do not use insecure randomness

    public static void Fill(this IFullMapping fullMapping)
    {
        ArgumentNullException.ThrowIfNull(fullMapping, nameof(fullMapping));

        var boolValue = ((long)NextDoubleRandom() % 2) > 0;
        var int8Value = (sbyte)(((long)(NextDoubleRandom() * 1000)) % sbyte.MaxValue);
        var uint8Value = (byte)(((long)(NextDoubleRandom() * 1000)) % byte.MaxValue);
        var int16Value = (short)(((long)(NextDoubleRandom() * 100000)) % short.MaxValue);
        var uint16Value = (ushort)(((long)(NextDoubleRandom() * 100000)) % ushort.MaxValue);
        var int32Value = (int)(((long)(NextDoubleRandom() * 10000000000)) % int.MaxValue);
        var uint32Value = (uint)(((long)(NextDoubleRandom() * 10000000000)) % uint.MaxValue);
        var int64Value = (long)(((long)(NextDoubleRandom() * 1000000000000000000)) % long.MaxValue);
        var uint64Value = (ulong)(((ulong)(NextDoubleRandom() * 1000000000000000000)) % ulong.MaxValue);
        var floatValue = (float)NextDoubleRandom();
        var doubleValue = (double)NextDoubleRandom();
        var decimalValue = (decimal)NextDoubleRandom();
        var dateTimeOffsetValue = DateTimeOffset.Now;
        var dateTimeValue = dateTimeOffsetValue.UtcDateTime;
        var dateOnlyValue = DateOnly.FromDateTime(dateTimeValue);
        var timeOnlyValue = TimeOnly.FromDateTime(dateTimeValue);
        var guidValue = Guid.CreateVersion7();
        var stringValue = $"{guidValue} {dateTimeOffsetValue}";
        var byteArrayValue = Encoding.UTF8.GetBytes(stringValue);
        var charArrayValue = Encoding.UTF8.GetChars(byteArrayValue);

        var type = typeof(IFullMapping);

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(bool)))
        {
            property.SetValue(fullMapping, boolValue);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(sbyte)))
        {
            property.SetValue(fullMapping, int8Value);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(byte)))
        {
            property.SetValue(fullMapping, uint8Value);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(short)))
        {
            property.SetValue(fullMapping, int16Value);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(ushort)))
        {
            property.SetValue(fullMapping, uint16Value);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(int)))
        {
            property.SetValue(fullMapping, int32Value);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(uint)))
        {
            property.SetValue(fullMapping, uint32Value);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(long)))
        {
            property.SetValue(fullMapping, int64Value);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(ulong)))
        {
            property.SetValue(fullMapping, uint64Value);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(float)))
        {
            property.SetValue(fullMapping, floatValue);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(double)))
        {
            property.SetValue(fullMapping, doubleValue);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(decimal)))
        {
            property.SetValue(fullMapping, decimalValue);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(DateOnly)))
        {
            property.SetValue(fullMapping, dateOnlyValue);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(TimeOnly)))
        {
            property.SetValue(fullMapping, timeOnlyValue);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(DateTime)))
        {
            property.SetValue(fullMapping, dateTimeValue);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset)))
        {
            property.SetValue(fullMapping, dateTimeOffsetValue);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(Guid)))
        {
            property.SetValue(fullMapping, guidValue);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(string)))
        {
            property.SetValue(fullMapping, stringValue);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(char[])))
        {
            property.SetValue(fullMapping, charArrayValue);
        }

        foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(byte[])))
        {
            property.SetValue(fullMapping, byteArrayValue);
        }

        fullMapping.FloatAsDecimal = (float)Decimal.Round((decimal)fullMapping.FloatAsDecimal, 7);
        fullMapping.FloatAsOACurrency = (float)Decimal.Round((decimal)fullMapping.FloatAsOACurrency, 4);
        fullMapping.FloatAsScientificString = (float)Decimal.Round((decimal)fullMapping.FloatAsScientificString, 7);

        fullMapping.DoubleAsSinglePrecisionFloatingPoint = (double)Decimal.Round((decimal)fullMapping.DoubleAsSinglePrecisionFloatingPoint, 7);
        fullMapping.DoubleAsDecimal = (double)Decimal.Round((decimal)fullMapping.DoubleAsDecimal, 12);
        fullMapping.DoubleAsOACurrency = (double)Decimal.Round((decimal)fullMapping.DoubleAsOACurrency, 4);
        fullMapping.DoubleAsScientificString = (double)Decimal.Round((decimal)fullMapping.DoubleAsScientificString, 12);

        fullMapping.DecimalAsSinglePrecisionFloatingPoint = (decimal)Decimal.Round((decimal)fullMapping.DecimalAsSinglePrecisionFloatingPoint, 7);
        fullMapping.DecimalAsOACurrency = (decimal)Decimal.Round((decimal)fullMapping.DecimalAsOACurrency, 4);

        fullMapping.TimeOnlyAsSerial = fullMapping.TimeOnlyAsSerial.TruncateToSeconds();
        fullMapping.TimeOnlyAsSerialString = fullMapping.TimeOnlyAsSerialString.TruncateToSeconds();

        fullMapping.DateTimeAsUtcDateTime = fullMapping.DateTimeAsUtcDateTime.ToUniversalTime();
        fullMapping.DateTimeAsLocalDateTime = fullMapping.DateTimeAsLocalDateTime.ToLocalTime();
        fullMapping.DateTimeAsUtcSerial = fullMapping.DateTimeAsUtcSerial.ToUniversalTime().TruncateToSeconds();
        fullMapping.DateTimeAsUtcSerialWithMilliseconds = fullMapping.DateTimeAsUtcSerialWithMilliseconds.ToUniversalTime().TruncateToMilliseconds();
        fullMapping.DateTimeAsUtcSerialString = fullMapping.DateTimeAsUtcSerialString.ToUniversalTime().TruncateToSeconds();
        fullMapping.DateTimeAsUtcSerialWithMillisecondsString = fullMapping.DateTimeAsUtcSerialWithMillisecondsString.ToUniversalTime().TruncateToMilliseconds();
        fullMapping.DateTimeAsUnixSeconds = fullMapping.DateTimeAsUnixSeconds.ToUniversalTime().TruncateToSeconds();
        fullMapping.DateTimeAsUnixMilliseconds = fullMapping.DateTimeAsUnixMilliseconds.ToUniversalTime().TruncateToMilliseconds();
        fullMapping.DateTimeAsOADateTime = fullMapping.DateTimeAsOADateTime.ToUniversalTime().TruncateToMilliseconds();
        fullMapping.DateTimeAsUtcString = fullMapping.DateTimeAsUtcString.ToUniversalTime();
        fullMapping.DateTimeAsLocalString = fullMapping.DateTimeAsLocalString.ToLocalTime();
        fullMapping.DateTimeAsString = fullMapping.DateTimeAsString.ToLocalTime();

        fullMapping.DateTimeOffsetAsUtcDateTime = fullMapping.DateTimeOffsetAsUtcDateTime.ToUniversalTime();
        fullMapping.DateTimeOffsetAsLocalDateTime = fullMapping.DateTimeOffsetAsLocalDateTime.ToLocalTime();
        fullMapping.DateTimeOffsetAsUtcSerial = fullMapping.DateTimeOffsetAsUtcSerial.ToUniversalTime().TruncateToSeconds();
        fullMapping.DateTimeOffsetAsUtcSerialWithMilliseconds = fullMapping.DateTimeOffsetAsUtcSerialWithMilliseconds.ToUniversalTime().TruncateToMilliseconds();
        fullMapping.DateTimeOffsetAsUtcSerialString = fullMapping.DateTimeOffsetAsUtcSerialString.ToUniversalTime().TruncateToSeconds();
        fullMapping.DateTimeOffsetAsUtcSerialWithMillisecondsString = fullMapping.DateTimeOffsetAsUtcSerialWithMillisecondsString.ToUniversalTime().TruncateToMilliseconds();
        fullMapping.DateTimeOffsetAsUnixSeconds = fullMapping.DateTimeOffsetAsUnixSeconds.ToUniversalTime().TruncateToSeconds();
        fullMapping.DateTimeOffsetAsUnixMilliseconds = fullMapping.DateTimeOffsetAsUnixMilliseconds.ToUniversalTime().TruncateToMilliseconds();
        fullMapping.DateTimeOffsetAsOADateTime = fullMapping.DateTimeOffsetAsOADateTime.ToUniversalTime().TruncateToMilliseconds();
        fullMapping.DateTimeOffsetAsUtcString = fullMapping.DateTimeOffsetAsUtcString.ToUniversalTime();
        fullMapping.DateTimeOffsetAsLocalString = fullMapping.DateTimeOffsetAsLocalString.ToLocalTime();
        fullMapping.DateTimeOffsetAsString = fullMapping.DateTimeOffsetAsString.ToLocalTime();

        fullMapping.ProtectedString = "FULLMAPPINGPROTECTED";
        fullMapping.ProtectedByteArray = Encoding.UTF8.GetBytes(fullMapping.ProtectedString);
        fullMapping.ProtectedCharArray = Encoding.UTF8.GetChars(fullMapping.ProtectedByteArray);
    }
}
