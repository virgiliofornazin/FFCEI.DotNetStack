using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ByteArrayColumnMappingTypeAttribute(ByteArrayColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new ByteArrayColumnMappingType MappingType { get; private set; } = mappingType;
}
