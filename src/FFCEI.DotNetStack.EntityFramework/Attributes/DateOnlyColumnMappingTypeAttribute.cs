using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DateOnlyColumnMappingTypeAttribute(DateOnlyColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new DateOnlyColumnMappingType MappingType { get; private set; } = mappingType;
}
