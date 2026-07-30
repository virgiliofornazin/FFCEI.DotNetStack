using FFCEI.DotNetStack.EntityFramework.Attributes;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.DataProtection;

public sealed class DataProtectionByteArrayValueConverter(DataProtectionAttribute dataProtectionAttribute, ConverterMappingHints? mappingHints = null)
    : ValueConverter<byte[], byte[]>(to => Protect(to, dataProtectionAttribute), from => Unprotect(from, dataProtectionAttribute), mappingHints)
{
    public DataProtectionAttribute DataProtectionAttribute { get; private set; } = dataProtectionAttribute;

    private static byte[] Protect(byte[] to, DataProtectionAttribute attribute)
    {
        var provider = IServiceProviderExtensionMethods.DataProtectionProvider ??
            throw DataProtectionChecks.NotInitialized();

        var protector = provider.CreateProtector(attribute.Purpose);

        return protector.Protect(to);
    }

    private static byte[] Unprotect(byte[] from, DataProtectionAttribute attribute)
    {
        var provider = IServiceProviderExtensionMethods.DataProtectionProvider ??
            throw DataProtectionChecks.NotInitialized();

        var protector = provider.CreateProtector(attribute.Purpose);

        return protector.Unprotect(from);
    }
}
