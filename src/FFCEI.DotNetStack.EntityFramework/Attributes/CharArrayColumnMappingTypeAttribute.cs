using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes;

namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class CharArrayColumnMappingTypeAttribute(CharArrayColumnMappingType mappingType)
    : MappingTypeAttribute(mappingType)
{
    public new CharArrayColumnMappingType MappingType { get; private set; } = mappingType;
}
