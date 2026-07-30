namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

public enum DoubleColumnMappingType
{
    None = 0,
    AsNumeric = 1,
    AsSinglePrecisionFloatingPoint = 2,
    AsDecimal = 3,
    AsOACurrency = 4,
    AsScientificString = 5,
    AsString = 6,
    Default = AsNumeric
}
