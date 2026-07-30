using FFCEI.DotNetStack.EntityFramework.Attributes;
using FFCEI.DotNetStack.EntityFramework.Storage.DataProtection;
using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes.Rules;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySql.EntityFrameworkCore.Extensions;
using System.Reflection;

namespace FFCEI.DotNetStack.EntityFramework;

public static class PropertyBuilderExtensionMethods
{
    internal static TPropertyBuilder DoApplyColumnProperties<TPropertyBuilder>(this TPropertyBuilder propertyBuilder, bool isView, DatabaseProviderType databaseProvider,
        object? mappingType = null) where TPropertyBuilder : PropertyBuilder
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder, nameof(propertyBuilder));

        var propertyName = propertyBuilder.Metadata.Name;
        var propertyInfo = propertyBuilder.Metadata.PropertyInfo;
        var propertyType = propertyBuilder.Metadata.ClrType;
        var propertyModelType = propertyType.NonNullableType();
        var propertyMappingRulesLookupType = propertyModelType.IsEnum ? typeof(long) : propertyModelType;

        if (!MappingRules.Rules.Value.TryGetValue(propertyMappingRulesLookupType, out var mappingRules) || (mappingRules == null))
        {
            throw new InvalidOperationException($"No mapping rules found for property {propertyName} [type {propertyModelType.CSharpTypeName()}]");
        }

        var noneValue = mappingRules.None;
        var defaultValue = mappingRules.Default;

        if (mappingType == null)
        {
            var mappingTypeAttribute = propertyInfo?.GetPropertyCustomAttributeFromEntityModelImplementationsOrInterface<MappingTypeAttribute>();

            if (mappingTypeAttribute != null)
            {
                mappingType = mappingTypeAttribute.MappingType;
            }
        }

        mappingType ??= defaultValue;

        if (mappingType == null)
        {
            throw new InvalidOperationException($"Invalid mapping rules found for property {propertyName} [type {propertyModelType.CSharpTypeName()}]");
        }

        if (noneValue == null)
        {
            throw new InvalidOperationException($"Incomplete mapping rules found for property {propertyName} [type {propertyModelType.CSharpTypeName()}]");
        }

        if (!Equals(mappingType.GetType(), noneValue.GetType()))
        {
            throw new InvalidOperationException($"Mismatch mapping rules found for property {propertyName} [type {propertyModelType.CSharpTypeName()}] : got {mappingType.GetType().Name} but it should be {noneValue.GetType().Name}");
        }

        if (Equals(mappingType, noneValue))
        {
            return propertyBuilder;
        }

        if (!mappingRules.TryGetMappingRuleFromMappingType(mappingType, out var mappingRule) || (mappingRule == null))
        {
            throw new InvalidOperationException($"Unknown mapping rule {mappingType} for property {propertyName} [type {propertyModelType.CSharpTypeName()}]");
        }

        mappingRule.PropertyBuilderAction?.Invoke(propertyBuilder, databaseProvider);

        var valueConverter = propertyBuilder.Metadata.GetValueConverter();

        var propertyModelTypeAfterConversion = valueConverter != null ? valueConverter.ProviderClrType : (propertyBuilder.Metadata.GetProviderClrType() ?? propertyModelType);
        var propertyDatabaseType = propertyModelTypeAfterConversion.NonNullableType();

        var dataProtectionResult = ApplyDataProtectionConversions(propertyBuilder, propertyInfo, propertyModelType, databaseProvider);

        if ((dataProtectionResult != null) && !dataProtectionResult.Value)
        {
            throw new InvalidOperationException($"[DataProtection] is not supported on provider {databaseProvider}; refusing to store '{propertyName}' in plaintext.");
        }

        if (isView)
        {
            return propertyBuilder;
        }

        string? columnType;
        string? collation;
        string? characterSet;

        if ((dataProtectionResult != null) && dataProtectionResult.Value)
        {
            columnType = DataProtectionColumnType(propertyModelType, databaseProvider);
            collation = null;
            characterSet = null;
        }
        else
        {
            var defaultColumnMappingAction = MappingRules.ColumnMappingActionOfType(propertyDatabaseType);

            (columnType, collation, characterSet) = (mappingRule.OverridenColumnMappingAction ?? defaultColumnMappingAction)(propertyBuilder, databaseProvider);
        }

        if (!string.IsNullOrWhiteSpace(columnType))
        {
            _ = propertyBuilder.HasColumnType(columnType);
        }

        switch (databaseProvider)
        {
        case DatabaseProviderType.Unknown:
            {
                break;
            }
        case DatabaseProviderType.InMemory:
            {
                break;
            }
        case DatabaseProviderType.Sqlite:
            {
                break;
            }
        case DatabaseProviderType.MySql:
            {
                if (!string.IsNullOrWhiteSpace(characterSet))
                {
                    _ = propertyBuilder.ForMySQLHasCharset(characterSet);
                }

                if (!string.IsNullOrWhiteSpace(collation))
                {
                    _ = propertyBuilder.ForMySQLHasCollation(collation);
                }

                break;
            }
        case DatabaseProviderType.Postgres:
            {
                break;
            }
        case DatabaseProviderType.Oracle:
            {
                break;
            }
        case DatabaseProviderType.SqlServer:
            {
                break;
            }
        default:
            {
                throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider.");
            }
        }

        if (!string.IsNullOrWhiteSpace(collation))
        {
            _ = propertyBuilder.UseCollation(collation);
        }

        return propertyBuilder;
    }

    private static string DataProtectionColumnType(Type propertyModelType, DatabaseProviderType databaseProvider)
    {
        var isText = (propertyModelType == typeof(string)) || (propertyModelType == typeof(char[]));

        if (!isText && (propertyModelType != typeof(byte[])))
        {
            throw new InvalidOperationException("DataProtection attribute is not valid for this property type");
        }

        return databaseProvider switch
        {
            DatabaseProviderType.Sqlite => isText ? "TEXT" : "BLOB",
            DatabaseProviderType.MySql => isText ? "LONGTEXT" : "LONGBLOB",
            DatabaseProviderType.Postgres => isText ? "TEXT" : "BYTEA",
            DatabaseProviderType.Oracle => isText ? "CLOB" : "BLOB",
            DatabaseProviderType.SqlServer => isText ? "NVARCHAR(MAX)" : "VARBINARY(MAX)",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };
    }

    public static TPropertyBuilder ApplyColumnProperties<TPropertyBuilder, TMappingType>(this TPropertyBuilder propertyBuilder, bool isView, DatabaseProviderType databaseProvider,
        TMappingType? mappingType = null) where TPropertyBuilder : PropertyBuilder where TMappingType : struct, Enum
            => DoApplyColumnProperties(propertyBuilder, isView, databaseProvider, mappingType);

    public static TPropertyBuilder ApplyDefaultColumnProperties<TPropertyBuilder>(this TPropertyBuilder propertyBuilder, bool isView, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
            => DoApplyColumnProperties(propertyBuilder, isView, databaseProvider);

    internal static bool HasDataProtectionConversion(PropertyInfo? propertyInfo)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        var dataProtectionAttribute = propertyInfo.GetPropertyCustomAttributeFromEntityModelImplementationsOrInterface<DataProtectionAttribute>();

        return (dataProtectionAttribute != null);
    }

    internal static bool HasDataProtectionConversion(this PropertyBuilder propertyBuilder)
        => HasDataProtectionConversion(propertyBuilder.Metadata.PropertyInfo);

    internal static bool? ApplyDataProtectionConversions(this PropertyBuilder propertyBuilder, PropertyInfo? propertyInfo, Type propertyModelType, DatabaseProviderType databaseProvider)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        var dataProtectionAttribute = propertyInfo.GetPropertyCustomAttributeFromEntityModelImplementationsOrInterface<DataProtectionAttribute>();

        if (dataProtectionAttribute == null)
        {
            return null;

        }

        var dataProtectionNotSupportedOnProvider =
            (databaseProvider == DatabaseProviderType.Unknown) ||
            (databaseProvider == DatabaseProviderType.InMemory);

        if (dataProtectionNotSupportedOnProvider)
        {
            return false;
        }

        ApplyDataProtectionConverter(propertyBuilder, propertyModelType, dataProtectionAttribute);

        return true;
    }

    private static void ApplyDataProtectionConverter(this PropertyBuilder propertyBuilder, Type propertyModelType, DataProtectionAttribute dataProtectionAttribute)
    {
        var valueConverter = propertyBuilder.Metadata.GetValueConverter();

        if (propertyModelType == typeof(string))
        {
            if (valueConverter != null)
            {
                throw new InvalidOperationException("DataProtection attribute is only valid if no ValueConverter was set");
            }

            _ = propertyBuilder.HasConversion(new DataProtectionStringValueConverter(dataProtectionAttribute));
        }
        else if (propertyModelType == typeof(char[]))
        {
            if ((valueConverter != null) &&
                (valueConverter.GetType().NamespaceAndName() != "FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion.CharArrayToStringConverter"))
            {
                throw new InvalidOperationException("DataProtection attribute is only valid if no ValueConverter was set");
            }

            _ = propertyBuilder.HasConversion(new DataProtectionCharArrayValueConverter(dataProtectionAttribute));

            propertyBuilder.Metadata.SetValueComparer(ValueComparer.CreateDefault<char[]>(favorStructuralComparisons: true));
        }
        else if (propertyModelType == typeof(byte[]))
        {
            if (valueConverter != null)
            {
                throw new InvalidOperationException("DataProtection attribute is only valid if no ValueConverter was set");
            }

            _ = propertyBuilder.HasConversion(new DataProtectionByteArrayValueConverter(dataProtectionAttribute));

            propertyBuilder.Metadata.SetValueComparer(ValueComparer.CreateDefault<byte[]>(favorStructuralComparisons: true));
        }
        else
        {
            throw new InvalidOperationException("DataProtection attribute is not valid for this property type");
        }
    }
}
