using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class TimeSpanColumnMappingTypeAttribute(TimeSpanColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new TimeSpanColumnMappingType MappingType { get; private set; } = mappingType;
}
