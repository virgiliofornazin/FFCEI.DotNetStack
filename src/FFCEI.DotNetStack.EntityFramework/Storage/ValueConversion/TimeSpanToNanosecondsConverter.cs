using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class TimeSpanToNanosecondsConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<TimeSpan, long>(static to => To(to), static from => From(from), mappingHints)
{
    public TimeSpanToNanosecondsConverter()
        : this(null)
    {
    }

    public static long To(TimeSpan to)
        => ValueConversions.TimeSpanToNanoseconds(to);

    public static TimeSpan From(long from)
        => ValueConversions.TimeSpanFromNanoseconds(from);
}
