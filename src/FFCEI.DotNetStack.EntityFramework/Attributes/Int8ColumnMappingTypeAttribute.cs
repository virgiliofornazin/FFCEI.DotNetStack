using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class Int8ColumnMappingTypeAttribute(Int8ColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new Int8ColumnMappingType MappingType { get; private set; } = mappingType;
}
