using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class TimeOnlyToNanosecondsConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<TimeOnly, long>(static to => To(to), static from => From(from), mappingHints)
{
    public TimeOnlyToNanosecondsConverter()
        : this(null)
    {
    }

    public static long To(TimeOnly to)
        => ValueConversions.TimeOnlyToNanoseconds(to);

    public static TimeOnly From(long from)
        => ValueConversions.TimeOnlyFromNanoseconds(from);
}
