namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

public enum ByteArrayColumnMappingType
{
    None = 0,
    AsBlob = 1,
    AsBase64String = 2,
    Default = AsBlob
}
