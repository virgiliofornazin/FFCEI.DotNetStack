namespace FFCEI.DotNetStack.Configuration.Configurations;

public sealed class EnvFileLoadOptions
{
    public bool SetEnvironmentVariables { get; set; }
    public bool IgnoreAlreadyDefinedVariables { get; set; }
    public bool AllowOverridePreviousVariables { get; set; }
}
