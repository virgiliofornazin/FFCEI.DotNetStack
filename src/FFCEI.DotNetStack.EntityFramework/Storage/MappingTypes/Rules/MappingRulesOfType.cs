using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes.Rules.Interfaces;

namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes.Rules;

internal abstract class MappingRulesOfType
    : IMappingRulesOfType
{
    public abstract Type ClrType { get; }
    public abstract object None { get; }
    public abstract object Default { get; }

    public abstract bool TryGetMappingRuleFromMappingType(object mappingType, out IMappingRuleOfType? mappingRule);

    public abstract void ForEachMappingType(Action<object, IMappingRuleOfType> callback);
}
