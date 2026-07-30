using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DecimalColumnMappingTypeAttribute(DecimalColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new DecimalColumnMappingType MappingType { get; private set; } = mappingType;
}
