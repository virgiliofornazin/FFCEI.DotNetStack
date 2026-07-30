namespace FFCEI.DotNetStack.Models.Reflection;

public sealed class ForeignKeyReferenceNavigationMetadata
{
    internal ForeignKeyReferenceNavigationMetadata(Type foreignEntityModelType, ForeignKeyReferenceMetadata foreignKeyReference)
    {
        ForeignEntityModelType = foreignEntityModelType;
        ForeignKeyReference = foreignKeyReference;
    }

    public Type ForeignEntityModelType { get; private set; }
    public ForeignKeyReferenceMetadata ForeignKeyReference { get; private set; }
}
