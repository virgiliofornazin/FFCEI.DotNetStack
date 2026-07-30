namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

public enum BoolColumnMappingType
{
    None = 0,
    AsBoolean = 1,
    AsNumeric = 2,
    AsYNString = 3,
    AsYesNoString = 4,
    AsTFString = 5,
    AsTrueFalseString = 6,
    Default = AsBoolean
}
