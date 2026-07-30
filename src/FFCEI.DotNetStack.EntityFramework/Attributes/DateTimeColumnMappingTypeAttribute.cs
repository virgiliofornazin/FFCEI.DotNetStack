using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DateTimeColumnMappingTypeAttribute(DateTimeColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new DateTimeColumnMappingType MappingType { get; private set; } = mappingType;
}
