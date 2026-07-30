using FFCEI.DotNetStack.EntityFramework.Attributes;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text;

namespace FFCEI.DotNetStack.EntityFramework.Storage.DataProtection;

public sealed class DataProtectionCharArrayValueConverter(DataProtectionAttribute dataProtectionAttribute, ConverterMappingHints? mappingHints = null)
    : ValueConverter<char[], string>(to => Protect(to, dataProtectionAttribute), from => Unprotect(from, dataProtectionAttribute), mappingHints)
{
    public DataProtectionAttribute DataProtectionAttribute { get; private set; } = dataProtectionAttribute;

    private static string Protect(char[] to, DataProtectionAttribute attribute)
    {
        var provider = IServiceProviderExtensionMethods.DataProtectionProvider ??
            throw DataProtectionChecks.NotInitialized();

        var protector = provider.CreateProtector(attribute.Purpose);

        var plaintextAsUTF8Bytes = Encoding.UTF8.GetBytes(to);
        var protectedDataAsBytes = protector.Protect(plaintextAsUTF8Bytes);

        return Convert.ToBase64String(protectedDataAsBytes);
    }

    private static char[] Unprotect(string from, DataProtectionAttribute attribute)
    {
        var provider = IServiceProviderExtensionMethods.DataProtectionProvider ??
            throw DataProtectionChecks.NotInitialized();

        var protector = provider.CreateProtector(attribute.Purpose);

        var protectedDataAsBytes = Convert.FromBase64String(from);
        var plaintextAsUTF8Bytes = protector.Unprotect(protectedDataAsBytes);

        return Encoding.UTF8.GetChars(plaintextAsUTF8Bytes);
    }
}
