namespace FFCEI.DotNetStack.Secrets;

public sealed class SecretValueVersion
{
    public required string Version { get; set; }
    public bool IsCurrent { get; set; }
    public DateTimeOffset CreationTimestamp { get; set; }
    public string? Status { get; set; }
}
