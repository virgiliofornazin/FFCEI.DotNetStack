namespace FFCEI.DotNetStack.Models.Reflection;

public class UniqueKeyReferenceColumnMetadata
{
    internal UniqueKeyReferenceColumnMetadata(ModelPropertyMetadata propertyMetadata, bool isDescending)
    {
        PropertyMetadata = propertyMetadata;
        IsDescending = isDescending;
    }

    public ModelPropertyMetadata PropertyMetadata { get; private set; }
    public bool IsAscending => !IsDescending;
    public bool IsDescending { get; private set; }
}
