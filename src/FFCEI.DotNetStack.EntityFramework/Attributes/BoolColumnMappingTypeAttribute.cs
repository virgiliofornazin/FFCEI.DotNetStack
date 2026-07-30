using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BoolColumnMappingTypeAttribute(BoolColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new BoolColumnMappingType MappingType { get; private set; } = mappingType;
}
