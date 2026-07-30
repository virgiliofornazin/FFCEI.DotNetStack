namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public abstract class MappingTypeAttribute
    : Attribute
{
    public Enum MappingType { get; private set; }

    protected MappingTypeAttribute(object mappingType)
    {
        if (mappingType is Enum mappingTypeEnum)
        {
            MappingType = mappingTypeEnum;
        }
        else
        {
            throw new ArgumentException("mappingType must be an Enum", nameof(mappingType));
        }
    }
}
