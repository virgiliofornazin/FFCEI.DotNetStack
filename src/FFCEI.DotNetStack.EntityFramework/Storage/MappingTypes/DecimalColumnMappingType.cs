namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

public enum DecimalColumnMappingType
{
    None = 0,
    AsNumeric = 1,
    AsSinglePrecisionFloatingPoint = 2,
    AsDoublePrecisionFloatingPoint = 3,
    AsOACurrency = 4,
    AsString = 5,
    Default = AsNumeric
}
