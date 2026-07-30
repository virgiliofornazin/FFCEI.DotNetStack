using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DoubleColumnMappingTypeAttribute(DoubleColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new DoubleColumnMappingType MappingType { get; private set; } = mappingType;
}
