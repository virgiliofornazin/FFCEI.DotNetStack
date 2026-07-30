using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DateTimeOffsetColumnMappingTypeAttribute(DateTimeOffsetColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new DateTimeOffsetColumnMappingType MappingType { get; private set; } = mappingType;
}
