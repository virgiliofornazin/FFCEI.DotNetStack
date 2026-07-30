using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes.Rules.Interfaces;

namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes.Rules;

internal sealed class MappingRulesOf<TClrType, TColumnMappingType>
    : MappingRulesOfType where TColumnMappingType : struct, Enum
{
    public override Type ClrType
        => typeof(TClrType);

    public override object None
        => NoneValue;

    public override object Default
        => DefaultValue;

    public override bool TryGetMappingRuleFromMappingType(object mappingType, out IMappingRuleOfType? mappingRule)
    {
        ArgumentNullException.ThrowIfNull(mappingType, nameof(mappingType));

        if (mappingType is not TColumnMappingType columnMappingType)
        {
            throw new InvalidCastException($"Cannot cast mappingType {mappingType.GetType()} to TColumnMappingType {typeof(TColumnMappingType)}");
        }

        if (MappingRules.TryGetValue(columnMappingType, out var rule))
        {
            mappingRule = rule;
        }
        else
        {
            mappingRule = null;
        }

        return (mappingRule != null);
    }

    public override void ForEachMappingType(Action<object, IMappingRuleOfType> callback)
    {
        ArgumentNullException.ThrowIfNull(callback, nameof(callback));

        foreach (var keyValuePair in MappingRules)
        {
            callback(keyValuePair.Key, keyValuePair.Value);
        }
    }

    public SortedDictionary<TColumnMappingType, MappingRuleOfType> MappingRules { get; private set; } = new();

    public TColumnMappingType NoneValue { get; private set; }

    public TColumnMappingType DefaultValue { get; private set; }

    public MappingRulesOf(TColumnMappingType noneValue, TColumnMappingType defaultValue, MappingRuleOf<TColumnMappingType>[] mappingRules)
    {
        NoneValue = noneValue;
        DefaultValue = defaultValue;

        foreach (var mappingRule in mappingRules)
        {
            MappingRules[mappingRule.ColumnMappingType] = mappingRule;
        }
    }
}
