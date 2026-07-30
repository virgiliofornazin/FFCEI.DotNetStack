using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateOnlyToDateTimeConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<DateOnly, DateTime>(static to => To(to), static from => From(from), mappingHints)
{
    public DateOnlyToDateTimeConverter()
        : this(null)
    {
    }

    public static DateTime To(DateOnly to)
        => new DateTime(to.Year, to.Month, to.Day, 0, 0, 0, DateTimeKind.Unspecified);

    public static DateOnly From(DateTime from)
        => new DateOnly(from.Year, from.Month, from.Day);
}
