namespace FFCEI.DotNetStack.EntityModels.Reflection;

public class UniqueKeyReferenceColumnMetadata
{
    internal UniqueKeyReferenceColumnMetadata(EntityModelPropertyMetadata propertyMetadata, bool isDescending)
    {
        PropertyMetadata = propertyMetadata;
        IsDescending = isDescending;
    }

    public EntityModelPropertyMetadata PropertyMetadata { get; private set; }
    public bool IsAscending => !IsDescending;
    public bool IsDescending { get; private set; }
}
