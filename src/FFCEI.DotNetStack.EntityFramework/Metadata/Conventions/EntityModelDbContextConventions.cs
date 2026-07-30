using FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.EntityFramework.Metadata.Conventions;

public static class EntityModelDbContextConventions
{
    internal static RecoverLogicallyDeletableStrategy RecoverStrategy { get; set; } = RecoverLogicallyDeletableStrategy.DetachExistingAttachNew;

    public static int MaxBlobLength { get; set; } = 1 * 1024 * 1024 * 1024;
    public static bool RemoveForeignKeyIndexConvertions { get; set; }

    private static PropertiesConfigurationBuilder[] PropertiesAndNullablePropertiesOfRawType<T>(this ModelConfigurationBuilder configurationBuilder) where T : struct
    {
        var properties = configurationBuilder.Properties<T>();
        var nullableProperties = configurationBuilder.Properties<T?>();

        return [properties, nullableProperties];
    }

    private static PropertiesConfigurationBuilder[] PropertiesAndNullablePropertiesOfClrType<T>(this ModelConfigurationBuilder configurationBuilder) where T : class
    {
        var properties = configurationBuilder.Properties<T>();

        return [properties];
    }

    internal static void ConfigureConventions(ModelConfigurationBuilder configurationBuilder, DatabaseProviderType databaseProvider)
    {
        configurationBuilder.Conventions.Add(serviceProvider => new AdjustIndexNamingConvention(
            serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<AdjustIndexNamingConvention>()));

        if (RemoveForeignKeyIndexConvertions)
        {
            configurationBuilder.Conventions.Remove<ForeignKeyIndexConvention>();
        }
        /*
        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<bool>())
        {
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<sbyte>())
        {
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<byte>())
        {
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<ushort>())
        {
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<short>())
        {
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<uint>())
        {
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<int>())
        {
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<ulong>())
        {
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<long>())
        {
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<float>())
        {
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<double>())
        {
        }
        */
        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<decimal>())
        {
            _ = properties.HavePrecision(38, 18);
        }
        /*
        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<DateOnly>())
        {
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<TimeOnly>())
        {
        }
        */
        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<DateTime>())
        {
            _ = properties.HaveConversion<DateTimeToUtcConverter>();
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<DateTimeOffset>())
        {
            _ = properties.HaveConversion<DateTimeOffsetToUtcDateTimeConverter>();
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<TimeSpan>())
        {
            _ = properties.HaveConversion<TimeSpanToTicksConverter>();
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfRawType<Guid>())
        {
            _ = properties.HaveConversion<GuidToStringConverter>().HaveMaxLength(36).AreUnicode(false).AreFixedLength(true);
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfClrType<string>())
        {
            _ = properties.AreUnicode(true).AreFixedLength(false);
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfClrType<char[]>())
        {
            _ = properties.AreUnicode(true).AreFixedLength(false);
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfClrType<byte[]>())
        {
            _ = properties.AreFixedLength(false);
        }

        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfClrType<Enum>())
        {
            _ = properties.HaveConversion<long>();
        }
        /*
        foreach (var properties in configurationBuilder.PropertiesAndNullablePropertiesOfClrType<SqlVector<float>>())
        {
        }
        */
    }
}
