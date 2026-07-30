using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class TimeOnlyToSerialWithNanosecondsConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<TimeOnly, long>(static to => To(to), static from => From(from), mappingHints)
{
    public TimeOnlyToSerialWithNanosecondsConverter()
        : this(null)
    {
    }

    public static long To(TimeOnly to)
        => ValueConversions.TimeOnlyToSerialWithNanoseconds(to);

    public static TimeOnly From(long from)
        => ValueConversions.TimeOnlyFromSerialWithNanoseconds(from);
}
