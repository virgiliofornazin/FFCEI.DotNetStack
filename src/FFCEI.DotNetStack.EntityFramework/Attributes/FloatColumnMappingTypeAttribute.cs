using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class FloatColumnMappingTypeAttribute(FloatColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new FloatColumnMappingType MappingType { get; private set; } = mappingType;
}
