namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

public enum DateTimeColumnMappingType
{
    None = 0,
    AsUtcDateTime = 1,
    AsLocalDateTime = 2,
    AsUtcSerial = 3,
    AsUtcSerialWithMilliseconds = 4,
    AsUtcSerialString = 5,
    AsUtcSerialWithMillisecondsString = 6,
    AsUnixSeconds = 7,
    AsUnixMilliseconds = 8,
    AsOADateTime = 9,
    AsUtcString = 10,
    AsLocalString = 11,
    AsISOString = 12,
    AsString = AsUtcString,
    Default = AsUtcDateTime
}
