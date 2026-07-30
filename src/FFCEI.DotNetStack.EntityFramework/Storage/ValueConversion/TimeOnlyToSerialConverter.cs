using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class TimeOnlyToSerialConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<TimeOnly, int>(static to => To(to), static from => From(from), mappingHints)
{
    public TimeOnlyToSerialConverter()
        : this(null)
    {
    }

    public static int To(TimeOnly to)
        => ValueConversions.TimeOnlyToSerial(to);

    public static TimeOnly From(int from)
        => ValueConversions.TimeOnlyFromSerial(from);
}
