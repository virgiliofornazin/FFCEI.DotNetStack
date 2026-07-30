using FFCEI.DotNetStack.EntityFramework.Attributes;
using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;
using FFCEI.DotNetStack.Models;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.TestModels;

public abstract class BaseFullMapping
    : UniqueIdExternalUniqueIdLogicallyDeletableTimestampTrackedEntityModel, IFullMapping
{
    [BoolColumnMappingType(BoolColumnMappingType.Default)]
    public bool BoolDefault { get; set; }
    [BoolColumnMappingType(BoolColumnMappingType.AsNumeric)]
    public bool BoolAsNumeric { get; set; }
    [BoolColumnMappingType(BoolColumnMappingType.AsYNString)]
    public bool BoolAsYNString { get; set; }
    [BoolColumnMappingType(BoolColumnMappingType.AsYesNoString)]
    public bool BoolAsYesNoString { get; set; }
    [BoolColumnMappingType(BoolColumnMappingType.AsTFString)]
    public bool BoolAsTFString { get; set; }
    [BoolColumnMappingType(BoolColumnMappingType.AsTrueFalseString)]
    public bool BoolAsTrueFalseString { get; set; }

    [Int8ColumnMappingType(Int8ColumnMappingType.Default)]
    public sbyte Int8Default { get; set; }
    [Int8ColumnMappingType(Int8ColumnMappingType.AsNumeric)]
    public sbyte Int8Numeric { get; set; }
    [Int8ColumnMappingType(Int8ColumnMappingType.AsString)]
    public sbyte Int8String { get; set; }

    [UInt8ColumnMappingType(UInt8ColumnMappingType.Default)]
    public byte UInt8Default { get; set; }
    [UInt8ColumnMappingType(UInt8ColumnMappingType.AsNumeric)]
    public byte UInt8Numeric { get; set; }
    [UInt8ColumnMappingType(UInt8ColumnMappingType.AsString)]
    public byte UInt8String { get; set; }

    [Int16ColumnMappingType(Int16ColumnMappingType.Default)]
    public short Int16Default { get; set; }
    [Int16ColumnMappingType(Int16ColumnMappingType.AsNumeric)]
    public short Int16Numeric { get; set; }
    [Int16ColumnMappingType(Int16ColumnMappingType.AsString)]
    public short Int16String { get; set; }

    [UInt16ColumnMappingType(UInt16ColumnMappingType.Default)]
    public ushort UInt16Default { get; set; }
    [UInt16ColumnMappingType(UInt16ColumnMappingType.AsNumeric)]
    public ushort UInt16Numeric { get; set; }
    [UInt16ColumnMappingType(UInt16ColumnMappingType.AsString)]
    public ushort UInt16String { get; set; }

    [Int32ColumnMappingType(Int32ColumnMappingType.Default)]
    public int Int32Default { get; set; }
    [Int32ColumnMappingType(Int32ColumnMappingType.AsNumeric)]
    public int Int32Numeric { get; set; }
    [Int32ColumnMappingType(Int32ColumnMappingType.AsString)]
    public int Int32String { get; set; }

    [UInt32ColumnMappingType(UInt32ColumnMappingType.Default)]
    public uint UInt32Default { get; set; }
    [UInt32ColumnMappingType(UInt32ColumnMappingType.AsNumeric)]
    public uint UInt32Numeric { get; set; }
    [UInt32ColumnMappingType(UInt32ColumnMappingType.AsString)]
    public uint UInt32String { get; set; }

    [Int64ColumnMappingType(Int64ColumnMappingType.Default)]
    public long Int64Default { get; set; }
    [Int64ColumnMappingType(Int64ColumnMappingType.AsNumeric)]
    public long Int64Numeric { get; set; }
    [Int64ColumnMappingType(Int64ColumnMappingType.AsString)]
    public long Int64String { get; set; }

    [UInt64ColumnMappingType(UInt64ColumnMappingType.Default)]
    public ulong UInt64Default { get; set; }
    [UInt64ColumnMappingType(UInt64ColumnMappingType.AsNumeric)]
    public ulong UInt64Numeric { get; set; }
    [UInt64ColumnMappingType(UInt64ColumnMappingType.AsString)]
    public ulong UInt64String { get; set; }

    [FloatColumnMappingType(FloatColumnMappingType.Default)]
    public float FloatDefault { get; set; }
    [FloatColumnMappingType(FloatColumnMappingType.AsNumeric)]
    public float FloatAsNumeric { get; set; }
    [FloatColumnMappingType(FloatColumnMappingType.AsDoublePrecisionFloatingPoint)]
    public float FloatAsDoublePrecisionFloatingPoint { get; set; }
    [FloatColumnMappingType(FloatColumnMappingType.AsDecimal)]
    public float FloatAsDecimal { get; set; }
    [FloatColumnMappingType(FloatColumnMappingType.AsOACurrency)]
    public float FloatAsOACurrency { get; set; }
    [FloatColumnMappingType(FloatColumnMappingType.AsScientificString)]
    public float FloatAsScientificString { get; set; }
    [FloatColumnMappingType(FloatColumnMappingType.AsString)]
    public float FloatAsString { get; set; }

    [DoubleColumnMappingType(DoubleColumnMappingType.Default)]
    public double DoubleDefault { get; set; }
    [DoubleColumnMappingType(DoubleColumnMappingType.AsNumeric)]
    public double DoubleAsNumeric { get; set; }
    [DoubleColumnMappingType(DoubleColumnMappingType.AsSinglePrecisionFloatingPoint)]
    public double DoubleAsSinglePrecisionFloatingPoint { get; set; }
    [DoubleColumnMappingType(DoubleColumnMappingType.AsDecimal)]
    public double DoubleAsDecimal { get; set; }
    [DoubleColumnMappingType(DoubleColumnMappingType.AsOACurrency)]
    public double DoubleAsOACurrency { get; set; }
    [DoubleColumnMappingType(DoubleColumnMappingType.AsScientificString)]
    public double DoubleAsScientificString { get; set; }
    [DoubleColumnMappingType(DoubleColumnMappingType.AsString)]
    public double DoubleAsString { get; set; }

    [DecimalColumnMappingType(DecimalColumnMappingType.Default)]
    public decimal DecimalDefault { get; set; }
    [DecimalColumnMappingType(DecimalColumnMappingType.AsNumeric)]
    public decimal DecimalAsNumeric { get; set; }
    [DecimalColumnMappingType(DecimalColumnMappingType.AsSinglePrecisionFloatingPoint)]
    public decimal DecimalAsSinglePrecisionFloatingPoint { get; set; }
    [DecimalColumnMappingType(DecimalColumnMappingType.AsDoublePrecisionFloatingPoint)]
    public decimal DecimalAsDoublePrecisionFloatingPoint { get; set; }
    [DecimalColumnMappingType(DecimalColumnMappingType.AsOACurrency)]
    public decimal DecimalAsOACurrency { get; set; }
    [DecimalColumnMappingType(DecimalColumnMappingType.AsString)]
    public decimal DecimalAsString { get; set; }

    [DateOnlyColumnMappingType(DateOnlyColumnMappingType.Default)]
    public DateOnly DateOnlyDefault { get; set; }
    [DateOnlyColumnMappingType(DateOnlyColumnMappingType.AsDate)]
    public DateOnly DateOnlyAsDate { get; set; }
    [DateOnlyColumnMappingType(DateOnlyColumnMappingType.AsSerial)]
    public DateOnly DateOnlyAsSerial { get; set; }
    [DateOnlyColumnMappingType(DateOnlyColumnMappingType.AsSerialString)]
    public DateOnly DateOnlyAsSerialString { get; set; }
    [DateOnlyColumnMappingType(DateOnlyColumnMappingType.AsOADateTime)]
    public DateOnly DateOnlyAsOADateTime { get; set; }
    [DateOnlyColumnMappingType(DateOnlyColumnMappingType.AsISOString)]
    public DateOnly DateOnlyAsISOString { get; set; }
    [DateOnlyColumnMappingType(DateOnlyColumnMappingType.AsString)]
    public DateOnly DateOnlyAsString { get; set; }

    [TimeOnlyColumnMappingType(TimeOnlyColumnMappingType.Default)]
    public TimeOnly TimeOnlyDefault { get; set; }
    [TimeOnlyColumnMappingType(TimeOnlyColumnMappingType.AsTime)]
    public TimeOnly TimeOnlyAsTime { get; set; }
    [TimeOnlyColumnMappingType(TimeOnlyColumnMappingType.AsNanoseconds)]
    public TimeOnly TimeOnlyAsNanosecondse { get; set; }
    [TimeOnlyColumnMappingType(TimeOnlyColumnMappingType.AsTicks)]
    public TimeOnly TimeOnlyAsTicks { get; set; }
    [TimeOnlyColumnMappingType(TimeOnlyColumnMappingType.AsTicksString)]
    public TimeOnly TimeOnlyAsTicksString { get; set; }
    [TimeOnlyColumnMappingType(TimeOnlyColumnMappingType.AsSerial)]
    public TimeOnly TimeOnlyAsSerial { get; set; }
    [TimeOnlyColumnMappingType(TimeOnlyColumnMappingType.AsSerialString)]
    public TimeOnly TimeOnlyAsSerialString { get; set; }
    [TimeOnlyColumnMappingType(TimeOnlyColumnMappingType.AsSerialWithNanoseconds)]
    public TimeOnly TimeOnlyAsSerialWithNanoseconds { get; set; }
    [TimeOnlyColumnMappingType(TimeOnlyColumnMappingType.AsSerialWithNanosecondsString)]
    public TimeOnly TimeOnlyAsSerialWithNanosecondsString { get; set; }
    [TimeOnlyColumnMappingType(TimeOnlyColumnMappingType.AsISOString)]
    public TimeOnly TimeOnlyAsISOString { get; set; }
    [TimeOnlyColumnMappingType(TimeOnlyColumnMappingType.AsString)]
    public TimeOnly TimeOnlyAsString { get; set; }

    [DateTimeColumnMappingType(DateTimeColumnMappingType.Default)]
    public DateTime DateTimeDefault { get; set; }
    [DateTimeColumnMappingType(DateTimeColumnMappingType.AsUtcDateTime)]
    public DateTime DateTimeAsUtcDateTime { get; set; }
    [DateTimeColumnMappingType(DateTimeColumnMappingType.AsLocalDateTime)]
    public DateTime DateTimeAsLocalDateTime { get; set; }
    [DateTimeColumnMappingType(DateTimeColumnMappingType.AsUtcSerial)]
    public DateTime DateTimeAsUtcSerial { get; set; }
    [DateTimeColumnMappingType(DateTimeColumnMappingType.AsUtcSerialWithMilliseconds)]
    public DateTime DateTimeAsUtcSerialWithMilliseconds { get; set; }
    [DateTimeColumnMappingType(DateTimeColumnMappingType.AsUtcSerialString)]
    public DateTime DateTimeAsUtcSerialString { get; set; }
    [DateTimeColumnMappingType(DateTimeColumnMappingType.AsUtcSerialWithMillisecondsString)]
    public DateTime DateTimeAsUtcSerialWithMillisecondsString { get; set; }
    [DateTimeColumnMappingType(DateTimeColumnMappingType.AsUnixSeconds)]
    public DateTime DateTimeAsUnixSeconds { get; set; }
    [DateTimeColumnMappingType(DateTimeColumnMappingType.AsUnixMilliseconds)]
    public DateTime DateTimeAsUnixMilliseconds { get; set; }
    [DateTimeColumnMappingType(DateTimeColumnMappingType.AsOADateTime)]
    public DateTime DateTimeAsOADateTime { get; set; }
    [DateTimeColumnMappingType(DateTimeColumnMappingType.AsUtcString)]
    public DateTime DateTimeAsUtcString { get; set; }
    [DateTimeColumnMappingType(DateTimeColumnMappingType.AsLocalString)]
    public DateTime DateTimeAsLocalString { get; set; }
    [DateTimeColumnMappingType(DateTimeColumnMappingType.AsISOString)]
    public DateTime DateTimeAsISOString { get; set; }
    [DateTimeColumnMappingType(DateTimeColumnMappingType.AsString)]
    public DateTime DateTimeAsString { get; set; }

    [DateTimeOffsetColumnMappingType(DateTimeOffsetColumnMappingType.Default)]
    public DateTimeOffset DateTimeOffsetDefault { get; set; }
    [DateTimeOffsetColumnMappingType(DateTimeOffsetColumnMappingType.AsDateTimeOffset)]
    public DateTimeOffset DateTimeOffsetAsDateTimeOffset { get; set; }
    [DateTimeOffsetColumnMappingType(DateTimeOffsetColumnMappingType.AsUtcDateTime)]
    public DateTimeOffset DateTimeOffsetAsUtcDateTime { get; set; }
    [DateTimeOffsetColumnMappingType(DateTimeOffsetColumnMappingType.AsLocalDateTime)]
    public DateTimeOffset DateTimeOffsetAsLocalDateTime { get; set; }
    [DateTimeOffsetColumnMappingType(DateTimeOffsetColumnMappingType.AsUtcSerial)]
    public DateTimeOffset DateTimeOffsetAsUtcSerial { get; set; }
    [DateTimeOffsetColumnMappingType(DateTimeOffsetColumnMappingType.AsUtcSerialWithMilliseconds)]
    public DateTimeOffset DateTimeOffsetAsUtcSerialWithMilliseconds { get; set; }
    [DateTimeOffsetColumnMappingType(DateTimeOffsetColumnMappingType.AsUtcSerialString)]
    public DateTimeOffset DateTimeOffsetAsUtcSerialString { get; set; }
    [DateTimeOffsetColumnMappingType(DateTimeOffsetColumnMappingType.AsUtcSerialWithMillisecondsString)]
    public DateTimeOffset DateTimeOffsetAsUtcSerialWithMillisecondsString { get; set; }
    [DateTimeOffsetColumnMappingType(DateTimeOffsetColumnMappingType.AsUnixSeconds)]
    public DateTimeOffset DateTimeOffsetAsUnixSeconds { get; set; }
    [DateTimeOffsetColumnMappingType(DateTimeOffsetColumnMappingType.AsUnixMilliseconds)]
    public DateTimeOffset DateTimeOffsetAsUnixMilliseconds { get; set; }
    [DateTimeOffsetColumnMappingType(DateTimeOffsetColumnMappingType.AsOADateTime)]
    public DateTimeOffset DateTimeOffsetAsOADateTime { get; set; }
    [DateTimeOffsetColumnMappingType(DateTimeOffsetColumnMappingType.AsUtcString)]
    public DateTimeOffset DateTimeOffsetAsUtcString { get; set; }
    [DateTimeOffsetColumnMappingType(DateTimeOffsetColumnMappingType.AsLocalString)]
    public DateTimeOffset DateTimeOffsetAsLocalString { get; set; }
    [DateTimeOffsetColumnMappingType(DateTimeOffsetColumnMappingType.AsISOString)]
    public DateTimeOffset DateTimeOffsetAsISOString { get; set; }
    [DateTimeOffsetColumnMappingType(DateTimeOffsetColumnMappingType.AsString)]
    public DateTimeOffset DateTimeOffsetAsString { get; set; }

    [TimeSpanColumnMappingType(TimeSpanColumnMappingType.Default)]
    public TimeSpan TimeSpanDefault { get; set; }
    [TimeSpanColumnMappingType(TimeSpanColumnMappingType.AsTimeSpan)]
    public TimeSpan TimeSpanAsTimeSpan { get; set; }
    [TimeSpanColumnMappingType(TimeSpanColumnMappingType.AsNanoseconds)]
    public TimeSpan TimeSpanAsNanoseconds { get; set; }
    [TimeSpanColumnMappingType(TimeSpanColumnMappingType.AsNanosecondsString)]
    public TimeSpan TimeSpanAsNanosecondsString { get; set; }
    [TimeSpanColumnMappingType(TimeSpanColumnMappingType.AsTicks)]
    public TimeSpan TimeSpanAsTicks { get; set; }
    [TimeSpanColumnMappingType(TimeSpanColumnMappingType.AsTicksString)]
    public TimeSpan TimeSpanAsTicksString { get; set; }
    [TimeSpanColumnMappingType(TimeSpanColumnMappingType.AsString)]
    public TimeSpan TimeSpanAsString { get; set; }

    [GuidColumnMappingType(GuidColumnMappingType.Default)]
    public Guid GuidDefault { get; set; }
    [GuidColumnMappingType(GuidColumnMappingType.AsUuid)]
    public Guid GuidAsUuid { get; set; }
    [GuidColumnMappingType(GuidColumnMappingType.AsBigEndianByteArray)]
    public Guid GuidAsBigEndianByteArray { get; set; }
    [GuidColumnMappingType(GuidColumnMappingType.AsCompactString)]
    public Guid GuidAsCompactString { get; set; }
    [GuidColumnMappingType(GuidColumnMappingType.AsString)]
    public Guid GuidAsString { get; set; }

    [StringColumnMappingType(StringColumnMappingType.Default)]
    public string StringDefault { get; set; } = string.Empty;
    [StringColumnMappingType(StringColumnMappingType.AsStringBlob)]
    public string StringAsStringBlob { get; set; } = string.Empty;
    [StringColumnMappingType(StringColumnMappingType.AsASCIIBinaryBlob)]
    public string StringAsASCIIBinaryBlob { get; set; } = string.Empty;
    [StringColumnMappingType(StringColumnMappingType.AsUTF8BinaryBlob)]
    public string StringAsUTF8BinaryBlob { get; set; } = string.Empty;
    [StringColumnMappingType(StringColumnMappingType.AsString)]
    public string StringAsString { get; set; } = string.Empty;

#pragma warning disable CA1819 // Properties should not return arrays
    [CharArrayColumnMappingType(CharArrayColumnMappingType.Default)]
    public char[] CharArrayDefault { get; set; } = [];
    [CharArrayColumnMappingType(CharArrayColumnMappingType.AsStringBlob)]
    public char[] CharArrayAsStringBlob { get; set; } = [];
    [CharArrayColumnMappingType(CharArrayColumnMappingType.AsASCIIBinaryBlob)]
    public char[] CharArrayAsASCIIBinaryBlob { get; set; } = [];
    [CharArrayColumnMappingType(CharArrayColumnMappingType.AsUTF8BinaryBlob)]
    public char[] CharArrayAsUTF8BinaryBlob { get; set; } = [];
    [CharArrayColumnMappingType(CharArrayColumnMappingType.AsString)]
    public char[] CharArrayAsString { get; set; } = [];

    [ByteArrayColumnMappingType(ByteArrayColumnMappingType.Default)]
    public byte[] ByteArrayDefault { get; set; } = [];
    [ByteArrayColumnMappingType(ByteArrayColumnMappingType.AsBlob)]
    public byte[] ByteArrayAsBlobt { get; set; } = [];
    [ByteArrayColumnMappingType(ByteArrayColumnMappingType.AsBase64String)]
    public byte[] ByteArrayAsBase64String { get; set; } = [];
#pragma warning restore CA1819 // Properties should not return arrays

    [DataProtection("StringPurpose")]
    public string ProtectedString { get; set; } = null!;

#pragma warning disable CA1819 // Properties should not return arrays
    [DataProtection("CharArrayPurpose")]
    public char[] ProtectedCharArray { get; set; } = [];

    [DataProtection("ByteArrayPurpose")]
    public byte[] ProtectedByteArray { get; set; } = [];
#pragma warning restore CA1819 // Properties should not return arrays
}
