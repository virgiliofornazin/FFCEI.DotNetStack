using FFCEI.DotNetStack.EntityFramework.Attributes;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.DataProtection;

public sealed class DataProtectionStringValueConverter(DataProtectionAttribute dataProtectionAttribute, ConverterMappingHints? mappingHints = null)
    : ValueConverter<string, string>(to => Protect(to, dataProtectionAttribute), from => Unprotect(from, dataProtectionAttribute), mappingHints)
{
    public DataProtectionAttribute DataProtectionAttribute { get; private set; } = dataProtectionAttribute;

    private static string Protect(string to, DataProtectionAttribute attribute)
    {
        var provider = IServiceProviderExtensionMethods.DataProtectionProvider ??
            throw DataProtectionChecks.NotInitialized();

        var protector = provider.CreateProtector(attribute.Purpose);

        return protector.Protect(to);
    }

    private static string Unprotect(string from, DataProtectionAttribute attribute)
    {
        var provider = IServiceProviderExtensionMethods.DataProtectionProvider ??
            throw DataProtectionChecks.NotInitialized();

        var protector = provider.CreateProtector(attribute.Purpose);

        return protector.Unprotect(from);
    }
}
