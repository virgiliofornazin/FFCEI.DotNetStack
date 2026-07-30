namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

public enum GuidColumnMappingType
{
    None = 0,
    AsUuid = 1,
    AsBigEndianByteArray = 2,
    AsCompactString = 3,
    AsString = 4,
    Default = AsString
}
