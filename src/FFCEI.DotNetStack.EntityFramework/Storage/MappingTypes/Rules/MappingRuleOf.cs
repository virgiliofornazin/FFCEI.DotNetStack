namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes.Rules;

internal sealed class MappingRuleOf<TColumnMappingType>
    : MappingRuleOfType where TColumnMappingType : struct, Enum
{
    public TColumnMappingType ColumnMappingType { get; set; }
}
