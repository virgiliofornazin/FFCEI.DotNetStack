namespace FFCEI.DotNetStack.EntityFramework.Storage.DataProtection;

internal static class DataProtectionChecks
{
    public static InvalidOperationException NotInitialized()
        => new InvalidOperationException("DataProtection provider not initialized; call AddDataProtectionVia... / UseDataProtection... first");
}
