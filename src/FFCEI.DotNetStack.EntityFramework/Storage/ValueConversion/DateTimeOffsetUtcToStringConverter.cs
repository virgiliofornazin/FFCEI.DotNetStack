using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeOffsetUtcToStringConverter
    : DateTimeOffsetToStringConverter
{
    private readonly Expression<Func<DateTimeOffset, string>> _convertToProvider;
    private readonly Expression<Func<string, DateTimeOffset>> _convertFromProvider;

    public DateTimeOffsetUtcToStringConverter()
        : this(null)
    {
    }

    public DateTimeOffsetUtcToStringConverter(ConverterMappingHints? mappingHints)
        : base(mappingHints)
    {
        var baseToProvider = base.ConvertToProviderExpression.Compile();
        var baseFromProvider = base.ConvertFromProviderExpression.Compile();

        _convertToProvider = to => baseToProvider(to.ToUniversalTime());
        _convertFromProvider = from => baseFromProvider(from).ToUniversalTime();
    }

    public override Expression<Func<DateTimeOffset, string>> ConvertToProviderExpression => _convertToProvider;
    public override Expression<Func<string, DateTimeOffset>> ConvertFromProviderExpression => _convertFromProvider;
}
