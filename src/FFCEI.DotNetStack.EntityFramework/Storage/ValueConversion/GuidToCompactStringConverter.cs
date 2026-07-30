using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class GuidToCompactStringConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<Guid, string>(static to => To(to), static from => From(from), mappingHints)
{
    public GuidToCompactStringConverter()
        : this(null)
    {
    }

    public static string To(Guid to)
        => ValueConversions.GuidToCompactString(to);

    public static Guid From(string from)
        => ValueConversions.GuidFromCompactString(from);
}
