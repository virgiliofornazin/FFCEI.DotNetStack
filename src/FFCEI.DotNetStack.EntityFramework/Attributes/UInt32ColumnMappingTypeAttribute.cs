using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UInt32ColumnMappingTypeAttribute(UInt32ColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new UInt32ColumnMappingType MappingType { get; private set; } = mappingType;
}
