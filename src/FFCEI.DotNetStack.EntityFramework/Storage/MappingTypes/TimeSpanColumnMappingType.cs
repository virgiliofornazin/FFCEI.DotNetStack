namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

public enum TimeSpanColumnMappingType
{
    None = 0,
    AsTimeSpan = 1,
    AsNanoseconds = 2,
    AsNanosecondsString = 3,
    AsTicks = 4,
    AsTicksString = 5,
    AsString = 6,
    Default = AsNanoseconds
}
