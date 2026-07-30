using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateOnlyToOADateTimeConverter(ConverterMappingHints? mappingHints)
    : ValueConverter<DateOnly, double>(static to => To(to), static from => From(from), mappingHints)
{
    public DateOnlyToOADateTimeConverter()
        : this(null)
    {
    }

    public static double To(DateOnly to)
    {
        if (to == DateOnly.MinValue)
        {
            throw new ArgumentOutOfRangeException(nameof(to), "DateOnly.MinValue cannot be represented as an OLE Automation date.");
        }

        return to.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.Zero), DateTimeKind.Utc).ToOADate();
    }

    public static DateOnly From(double from)
        => DateOnly.FromDateTime(DateTime.FromOADate(from));
}
