namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

public enum TimeOnlyColumnMappingType
{
    None = 0,
    AsTime = 1,
    AsNanoseconds = 2,
    AsTicks = 3,
    AsTicksString = 4,
    AsSerial = 5,
    AsSerialString = 6,
    AsSerialWithNanoseconds = 7,
    AsSerialWithNanosecondsString = 8,
    AsISOString = 9,
    AsString = 10,
    Default = AsTime
}
