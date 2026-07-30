using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class Int32ColumnMappingTypeAttribute(Int32ColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new Int32ColumnMappingType MappingType { get; private set; } = mappingType;
}
