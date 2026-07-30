namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

public enum FloatColumnMappingType
{
    None = 0,
    AsNumeric = 1,
    AsDoublePrecisionFloatingPoint = 2,
    AsDecimal = 3,
    AsOACurrency = 4,
    AsScientificString = 5,
    AsString = 6,
    Default = AsNumeric
}
