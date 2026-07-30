using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class Int16ColumnMappingTypeAttribute(Int16ColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new Int16ColumnMappingType MappingType { get; private set; } = mappingType;
}
