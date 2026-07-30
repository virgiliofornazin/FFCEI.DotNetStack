using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UInt64ColumnMappingTypeAttribute(UInt64ColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new UInt64ColumnMappingType MappingType { get; private set; } = mappingType;
}
