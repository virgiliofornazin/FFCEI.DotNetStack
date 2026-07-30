using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeOffsetLocalToStringConverter
    : DateTimeOffsetToStringConverter
{
    private readonly Expression<Func<DateTimeOffset, string>> _convertToProvider;
    private readonly Expression<Func<string, DateTimeOffset>> _convertFromProvider;

    public DateTimeOffsetLocalToStringConverter()
        : this(null)
    {
    }

    public DateTimeOffsetLocalToStringConverter(ConverterMappingHints? mappingHints)
        : base(mappingHints)
    {
        var baseToProvider = base.ConvertToProviderExpression.Compile();
        var baseFromProvider = base.ConvertFromProviderExpression.Compile();

        _convertToProvider = to => baseToProvider(to.ToLocalTime());
        _convertFromProvider = from => baseFromProvider(from).ToLocalTime();
    }

    public override Expression<Func<DateTimeOffset, string>> ConvertToProviderExpression
        => _convertToProvider;
    public override Expression<Func<string, DateTimeOffset>> ConvertFromProviderExpression
        => _convertFromProvider;
}
