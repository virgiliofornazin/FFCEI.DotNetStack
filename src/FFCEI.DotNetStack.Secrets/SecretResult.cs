namespace FFCEI.DotNetStack.Secrets;

public sealed class SecretResult
{
    public required string SecretName { get; set; }
    public required string CurrentVersion { get; set; }
    public string? PreviousVersion { get; set; }
}
