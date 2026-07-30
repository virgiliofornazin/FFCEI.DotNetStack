using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class TimeOnlyToTimeSpanConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<TimeOnly, TimeSpan>(static to => To(to), static from => From(from), mappingHints)
{
    public TimeOnlyToTimeSpanConverter()
        : this(null)
    {
    }

    public static TimeSpan To(TimeOnly to)
        => to.ToTimeSpan();

    public static TimeOnly From(TimeSpan from)
        => TimeOnly.FromTimeSpan(from);
}
