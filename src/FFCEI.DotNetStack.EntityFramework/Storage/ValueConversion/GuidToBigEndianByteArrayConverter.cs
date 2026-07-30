using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class GuidToBigEndianByteArrayConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<Guid, byte[]>(static to => To(to), static from => From(from), mappingHints)
{
    public GuidToBigEndianByteArrayConverter()
        : this(null)
    {
    }

    public static byte[] To(Guid to)
        => ValueConversions.GuidToBigEndianByteArray(to);

    public static Guid From(byte[] from)
        => ValueConversions.GuidFromBigEndianByteArray(from);
}
