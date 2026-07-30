namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

public enum DateOnlyColumnMappingType
{
    None = 0,
    AsDate = 1,
    AsSerial = 2,
    AsSerialString = 3,
    AsOADateTime = 4,
    AsISOString = 5,
    AsString = 6,
    Default = AsDate
}
