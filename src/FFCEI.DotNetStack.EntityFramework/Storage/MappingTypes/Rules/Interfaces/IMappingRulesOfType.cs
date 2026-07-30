namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes.Rules.Interfaces;

internal interface IMappingRulesOfType
{
    Type ClrType { get; }
    object None { get; }
    object Default { get; }

    bool TryGetMappingRuleFromMappingType(object mappingType, out IMappingRuleOfType? mappingRule);

    void ForEachMappingType(Action<object, IMappingRuleOfType> callback);
}
