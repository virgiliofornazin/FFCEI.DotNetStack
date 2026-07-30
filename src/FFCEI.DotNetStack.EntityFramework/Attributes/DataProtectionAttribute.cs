namespace FFCEI.DotNetStack.EntityFramework.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DataProtectionAttribute
    : Attribute
{
    public string Purpose { get; }

    public DataProtectionAttribute(string purpose)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose, nameof(purpose));

        Purpose = purpose;
    }
}
