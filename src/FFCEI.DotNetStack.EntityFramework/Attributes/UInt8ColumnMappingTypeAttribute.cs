using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UInt8ColumnMappingTypeAttribute(UInt8ColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new UInt8ColumnMappingType MappingType { get; private set; } = mappingType;
}
