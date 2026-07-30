using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class Int64ColumnMappingTypeAttribute(Int64ColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new Int64ColumnMappingType MappingType { get; private set; } = mappingType;
}
