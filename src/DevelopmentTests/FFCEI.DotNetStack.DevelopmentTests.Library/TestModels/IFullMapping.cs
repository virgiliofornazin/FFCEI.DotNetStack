using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.TestModels;

public interface IFullMapping
    : IUniqueIdEntityModel, IExternalUniqueIdEntityModel, ILogicallyDeletableEntityModel, ITimestampTrackedEntityModel
{
    bool BoolDefault { get; set; }
    bool BoolAsNumeric { get; set; }
    bool BoolAsYNString { get; set; }
    bool BoolAsYesNoString { get; set; }
    bool BoolAsTFString { get; set; }
    bool BoolAsTrueFalseString { get; set; }

    sbyte Int8Default { get; set; }
    sbyte Int8Numeric { get; set; }
    sbyte Int8String { get; set; }

    byte UInt8Default { get; set; }
    byte UInt8Numeric { get; set; }
    byte UInt8String { get; set; }

    short Int16Default { get; set; }
    short Int16Numeric { get; set; }
    short Int16String { get; set; }

    ushort UInt16Default { get; set; }
    ushort UInt16Numeric { get; set; }
    ushort UInt16String { get; set; }

    int Int32Default { get; set; }
    int Int32Numeric { get; set; }
    int Int32String { get; set; }

    uint UInt32Default { get; set; }
    uint UInt32Numeric { get; set; }
    uint UInt32String { get; set; }

    long Int64Default { get; set; }
    long Int64Numeric { get; set; }
    long Int64String { get; set; }

    ulong UInt64Default { get; set; }
    ulong UInt64Numeric { get; set; }
    ulong UInt64String { get; set; }

    float FloatDefault { get; set; }
    float FloatAsNumeric { get; set; }
    float FloatAsDoublePrecisionFloatingPoint { get; set; }
    float FloatAsDecimal { get; set; }
    float FloatAsOACurrency { get; set; }
    float FloatAsScientificString { get; set; }
    float FloatAsString { get; set; }

    double DoubleDefault { get; set; }
    double DoubleAsNumeric { get; set; }
    double DoubleAsSinglePrecisionFloatingPoint { get; set; }
    double DoubleAsDecimal { get; set; }
    double DoubleAsOACurrency { get; set; }
    double DoubleAsScientificString { get; set; }
    double DoubleAsString { get; set; }

    decimal DecimalDefault { get; set; }
    decimal DecimalAsNumeric { get; set; }
    decimal DecimalAsSinglePrecisionFloatingPoint { get; set; }
    decimal DecimalAsDoublePrecisionFloatingPoint { get; set; }
    decimal DecimalAsOACurrency { get; set; }
    decimal DecimalAsString { get; set; }

    DateOnly DateOnlyDefault { get; set; }
    DateOnly DateOnlyAsDate { get; set; }
    DateOnly DateOnlyAsSerial { get; set; }
    DateOnly DateOnlyAsSerialString { get; set; }
    DateOnly DateOnlyAsOADateTime { get; set; }
    DateOnly DateOnlyAsISOString { get; set; }
    DateOnly DateOnlyAsString { get; set; }

    TimeOnly TimeOnlyDefault { get; set; }
    TimeOnly TimeOnlyAsTime { get; set; }
    TimeOnly TimeOnlyAsNanosecondse { get; set; }
    TimeOnly TimeOnlyAsTicks { get; set; }
    TimeOnly TimeOnlyAsTicksString { get; set; }
    TimeOnly TimeOnlyAsSerial { get; set; }
    TimeOnly TimeOnlyAsSerialString { get; set; }
    TimeOnly TimeOnlyAsSerialWithNanoseconds { get; set; }
    TimeOnly TimeOnlyAsSerialWithNanosecondsString { get; set; }
    TimeOnly TimeOnlyAsISOString { get; set; }
    TimeOnly TimeOnlyAsString { get; set; }

    DateTime DateTimeDefault { get; set; }
    DateTime DateTimeAsUtcDateTime { get; set; }
    DateTime DateTimeAsLocalDateTime { get; set; }
    DateTime DateTimeAsUtcSerial { get; set; }
    DateTime DateTimeAsUtcSerialWithMilliseconds { get; set; }
    DateTime DateTimeAsUtcSerialString { get; set; }
    DateTime DateTimeAsUtcSerialWithMillisecondsString { get; set; }
    DateTime DateTimeAsUnixSeconds { get; set; }
    DateTime DateTimeAsUnixMilliseconds { get; set; }
    DateTime DateTimeAsOADateTime { get; set; }
    DateTime DateTimeAsUtcString { get; set; }
    DateTime DateTimeAsLocalString { get; set; }
    DateTime DateTimeAsISOString { get; set; }
    DateTime DateTimeAsString { get; set; }

    DateTimeOffset DateTimeOffsetDefault { get; set; }
    DateTimeOffset DateTimeOffsetAsDateTimeOffset { get; set; }
    DateTimeOffset DateTimeOffsetAsUtcDateTime { get; set; }
    DateTimeOffset DateTimeOffsetAsLocalDateTime { get; set; }
    DateTimeOffset DateTimeOffsetAsUtcSerial { get; set; }
    DateTimeOffset DateTimeOffsetAsUtcSerialWithMilliseconds { get; set; }
    DateTimeOffset DateTimeOffsetAsUtcSerialString { get; set; }
    DateTimeOffset DateTimeOffsetAsUtcSerialWithMillisecondsString { get; set; }
    DateTimeOffset DateTimeOffsetAsUnixSeconds { get; set; }
    DateTimeOffset DateTimeOffsetAsUnixMilliseconds { get; set; }
    DateTimeOffset DateTimeOffsetAsOADateTime { get; set; }
    DateTimeOffset DateTimeOffsetAsUtcString { get; set; }
    DateTimeOffset DateTimeOffsetAsLocalString { get; set; }
    DateTimeOffset DateTimeOffsetAsISOString { get; set; }
    DateTimeOffset DateTimeOffsetAsString { get; set; }

    TimeSpan TimeSpanDefault { get; set; }
    TimeSpan TimeSpanAsTimeSpan { get; set; }
    TimeSpan TimeSpanAsNanoseconds { get; set; }
    TimeSpan TimeSpanAsNanosecondsString { get; set; }
    TimeSpan TimeSpanAsTicks { get; set; }
    TimeSpan TimeSpanAsTicksString { get; set; }
    TimeSpan TimeSpanAsString { get; set; }

    Guid GuidDefault { get; set; }
    Guid GuidAsUuid { get; set; }
    Guid GuidAsBigEndianByteArray { get; set; }
    Guid GuidAsCompactString { get; set; }
    Guid GuidAsString { get; set; }

    string StringDefault { get; set; }
    string StringAsStringBlob { get; set; }
    string StringAsASCIIBinaryBlob { get; set; }
    string StringAsUTF8BinaryBlob { get; set; }
    string StringAsString { get; set; }

#pragma warning disable CA1819 // Properties should not return arrays
    char[] CharArrayDefault { get; set; }
    char[] CharArrayAsStringBlob { get; set; }
    char[] CharArrayAsASCIIBinaryBlob { get; set; }
    char[] CharArrayAsUTF8BinaryBlob { get; set; }
    char[] CharArrayAsString { get; set; }

    byte[] ByteArrayDefault { get; set; }
    byte[] ByteArrayAsBlobt { get; set; }
    byte[] ByteArrayAsBase64String { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

    string ProtectedString { get; set; }

#pragma warning disable CA1819 // Properties should not return arrays
    char[] ProtectedCharArray { get; set; }

    byte[] ProtectedByteArray { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
}
