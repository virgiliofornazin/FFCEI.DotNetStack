namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

public enum DateTimeOffsetColumnMappingType
{
    None = 0,
    AsDateTimeOffset = 1,
    AsUtcDateTime = 2,
    AsLocalDateTime = 3,
    AsUtcSerial = 4,
    AsUtcSerialWithMilliseconds = 5,
    AsUtcSerialString = 6,
    AsUtcSerialWithMillisecondsString = 7,
    AsUnixSeconds = 8,
    AsUnixMilliseconds = 9,
    AsOADateTime = 10,
    AsUtcString = 11,
    AsLocalString = 12,
    AsISOString = 13,
    AsString = 14,
    Default = AsDateTimeOffset
}
