using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class StringColumnMappingTypeAttribute(StringColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new StringColumnMappingType MappingType { get; private set; } = mappingType;
}
