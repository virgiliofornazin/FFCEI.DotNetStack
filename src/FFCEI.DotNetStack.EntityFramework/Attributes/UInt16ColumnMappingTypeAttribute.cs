using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UInt16ColumnMappingTypeAttribute(UInt16ColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new UInt16ColumnMappingType MappingType { get; private set; } = mappingType;
}
