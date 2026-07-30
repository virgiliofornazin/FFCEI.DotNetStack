using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeLocalToStringConverter
    : DateTimeToStringConverter
{
    private readonly Expression<Func<DateTime, string>> _convertToProvider;
    private readonly Expression<Func<string, DateTime>> _convertFromProvider;

    public DateTimeLocalToStringConverter()
        : this(null)
    {
    }

    public DateTimeLocalToStringConverter(ConverterMappingHints? mappingHints)
        : base(mappingHints)
    {
        var baseToProvider = base.ConvertToProviderExpression.Compile();
        var baseFromProvider = base.ConvertFromProviderExpression.Compile();

        _convertToProvider = to => baseToProvider(To(to));
        _convertFromProvider = from => From(baseFromProvider(from));
    }

    public static DateTime To(DateTime to)
        => (to.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(to, DateTimeKind.Local) : to).ToLocalTime();

    public static DateTime From(DateTime from)
        => DateTime.SpecifyKind(from, DateTimeKind.Local);

    public override Expression<Func<DateTime, string>> ConvertToProviderExpression
        => _convertToProvider;
    public override Expression<Func<string, DateTime>> ConvertFromProviderExpression
        => _convertFromProvider;
}
