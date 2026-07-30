namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

public enum StringColumnMappingType
{
    None = 0,
    AsStringBlob = 1,
    AsASCIIBinaryBlob = 2,
    AsUTF8BinaryBlob = 3,
    AsString = 4,
    Default = AsString
}
