using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;

public sealed class DateTimeUtcToStringConverter
    : DateTimeToStringConverter
{
    private readonly Expression<Func<DateTime, string>> _convertToProvider;
    private readonly Expression<Func<string, DateTime>> _convertFromProvider;

    public DateTimeUtcToStringConverter()
        : this(null)
    {
    }

    public DateTimeUtcToStringConverter(ConverterMappingHints? mappingHints)
        : base(mappingHints)
    {
        var baseToProvider = base.ConvertToProviderExpression.Compile();
        var baseFromProvider = base.ConvertFromProviderExpression.Compile();

        _convertToProvider = to => baseToProvider(ValueConversions.DateTimeAdjustToUtcIfNeeded(to));
        _convertFromProvider = from => ValueConversions.DateTimeAdjustToUtcIfNeeded(baseFromProvider(from));
    }

    public override Expression<Func<DateTime, string>> ConvertToProviderExpression => _convertToProvider;
    public override Expression<Func<string, DateTime>> ConvertFromProviderExpression => _convertFromProvider;
}
