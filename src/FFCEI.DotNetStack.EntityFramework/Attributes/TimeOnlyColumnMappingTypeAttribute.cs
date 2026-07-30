using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class TimeOnlyColumnMappingTypeAttribute(TimeOnlyColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new TimeOnlyColumnMappingType MappingType { get; private set; } = mappingType;
}
