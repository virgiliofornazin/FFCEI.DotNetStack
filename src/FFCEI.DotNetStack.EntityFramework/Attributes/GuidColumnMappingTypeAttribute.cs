using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class GuidColumnMappingTypeAttribute(GuidColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new GuidColumnMappingType MappingType { get; private set; } = mappingType;
}
