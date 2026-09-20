using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes.Rules.Interfaces;
using FFCEI.DotNetStack.EntityFramework.Storage.ValueConversion;
using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes.Rules;

public static class MappingRules
{
    public const string MySqlUnicodeCollation = "utf8mb4_0900_ai_ci";
    public const string MySqlUnicodeCharacterSet = "utf8mb4";
    public const int MySqlTimeMaxFractionalSecondDigits = 6;
    public const bool PostgresUnicode = true;
    public const string PostgresUnicodeCollation = "unicode_ci_ai_utf8_icu";
    public const string PostgresUnicodeLocale = "und-u-ks-level1-kc-true";
    public const int PostgresTimeMaxFractionalSecondDigits = 6;
    public const string SqlServerUnicodeCollation = "Latin1_General_100_CI_AI_SC_UTF8";
    public const int SqlServerTimeMaxFractionalSecondDigits = 7;
    public const string OracleUnicodeCollation = null!; /* "BINARY_CI" */
    public const int OracleTimeMaxFractionalSecondDigits = 9;

    internal static Lazy<IReadOnlyDictionary<Type, IMappingRulesOfType>> Rules = new(() =>
    {
        var rules = new Dictionary<Type, IMappingRulesOfType>()
        {
            [typeof(bool)] = new MappingRulesOf<bool, BoolColumnMappingType>(
                noneValue: BoolColumnMappingType.None,
                defaultValue: BoolColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = BoolColumnMappingType.AsBoolean,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            if (databaseProvider == DatabaseProviderType.Sqlite ||
                                databaseProvider == DatabaseProviderType.MySql)
                            {
                                _ = propertyBuilder.HasConversion(new BoolToZeroOneConverter<int>(
                                        (databaseProvider == DatabaseProviderType.MySql) ? new(precision: 1) : null
                                    ));
                            }
                        },
                        OverridenColumnMappingAction = BooleanColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = BoolColumnMappingType.AsNumeric,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new BoolToZeroOneConverter<int>(
                                    (databaseProvider == DatabaseProviderType.MySql ||
                                    databaseProvider == DatabaseProviderType.Oracle) ? new(precision: 1) : null
                                ));
                        },
                        OverridenColumnMappingAction = Int8ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = BoolColumnMappingType.AsYNString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new BoolToStringConverter("n", "y", new(size: 1, unicode: false))).IsFixedLength(); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = BoolColumnMappingType.AsYesNoString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new BoolToStringConverter("no", "yes", new(size: 3, unicode: false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = BoolColumnMappingType.AsTFString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new BoolToStringConverter("f", "t", new(size: 1, unicode: false))).IsFixedLength(); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = BoolColumnMappingType.AsTrueFalseString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new BoolToStringConverter("false", "true", new(size: 5, unicode: false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    }
                    ]),
            [typeof(sbyte)] = new MappingRulesOf<sbyte, Int8ColumnMappingType>(
                noneValue: Int8ColumnMappingType.None,
                defaultValue: Int8ColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = Int8ColumnMappingType.AsNumeric,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            if (databaseProvider == DatabaseProviderType.Oracle)
                            {
                                _ = propertyBuilder.HasPrecision(3);
                            }
                        },
                        OverridenColumnMappingAction = Int8ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = Int8ColumnMappingType.AsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion<string>().HasMaxLength(4).IsUnicode(false); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    }
                ]),
            [typeof(byte)] = new MappingRulesOf<byte, UInt8ColumnMappingType>(
                noneValue: UInt8ColumnMappingType.None,
                defaultValue: UInt8ColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = UInt8ColumnMappingType.AsNumeric,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            if (databaseProvider == DatabaseProviderType.Oracle)
                            {
                                _ = propertyBuilder.HasPrecision(3);
                            }
                        },
                        OverridenColumnMappingAction = UInt8ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = UInt8ColumnMappingType.AsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion<string>().HasMaxLength(3).IsUnicode(false); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    }
                ]),
            [typeof(short)] = new MappingRulesOf<short, Int16ColumnMappingType>(
                noneValue: Int16ColumnMappingType.None,
                defaultValue: Int16ColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = Int16ColumnMappingType.AsNumeric,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            if (databaseProvider == DatabaseProviderType.Oracle)
                            {
                                _ = propertyBuilder.HasPrecision(5);
                            }
                        },
                        OverridenColumnMappingAction = Int16ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = Int16ColumnMappingType.AsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion<string>().HasMaxLength(6).IsUnicode(false); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    }
                ]),
            [typeof(ushort)] = new MappingRulesOf<ushort, UInt16ColumnMappingType>(
                noneValue: UInt16ColumnMappingType.None,
                defaultValue: UInt16ColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = UInt16ColumnMappingType.AsNumeric,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            if (databaseProvider == DatabaseProviderType.Oracle)
                            {
                                _ = propertyBuilder.HasPrecision(5);
                            }
                        },
                        OverridenColumnMappingAction = UInt16ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = UInt16ColumnMappingType.AsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion<string>().HasMaxLength(5).IsUnicode(false); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    }
                ]),
            [typeof(int)] = new MappingRulesOf<int, Int32ColumnMappingType>(
                noneValue: Int32ColumnMappingType.None,
                defaultValue: Int32ColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = Int32ColumnMappingType.AsNumeric,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            if (databaseProvider == DatabaseProviderType.Oracle)
                            {
                                _ = propertyBuilder.HasPrecision(10);
                            }
                        },
                        OverridenColumnMappingAction = Int32ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = Int32ColumnMappingType.AsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion<string>().HasMaxLength(11).IsUnicode(false); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    }
                ]),
            [typeof(uint)] = new MappingRulesOf<uint, UInt32ColumnMappingType>(
                noneValue: UInt32ColumnMappingType.None,
                defaultValue: UInt32ColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = UInt32ColumnMappingType.AsNumeric,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            if (databaseProvider == DatabaseProviderType.Oracle)
                            {
                                _ = propertyBuilder.HasPrecision(10);
                            }
                        },
                        OverridenColumnMappingAction = UInt32ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = UInt32ColumnMappingType.AsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion<string>().HasMaxLength(10).IsUnicode(false); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    }
                ]),
            [typeof(long)] = new MappingRulesOf<long, Int64ColumnMappingType>(
                noneValue: Int64ColumnMappingType.None,
                defaultValue: Int64ColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = Int64ColumnMappingType.AsNumeric,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            if (databaseProvider == DatabaseProviderType.Oracle)
                            {
                                _ = propertyBuilder.HasPrecision(19);
                            }
                        },
                        OverridenColumnMappingAction = Int64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = Int64ColumnMappingType.AsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion<string>().HasMaxLength(20).IsUnicode(false); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    }
                ]),
            [typeof(ulong)] = new MappingRulesOf<ulong, UInt64ColumnMappingType>(
                noneValue: UInt64ColumnMappingType.None,
                defaultValue: UInt64ColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = UInt64ColumnMappingType.AsNumeric,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            if (databaseProvider == DatabaseProviderType.Sqlite)
                            {
                                _ = propertyBuilder.HasConversion<ulong>();
                            }
                            else if (databaseProvider == DatabaseProviderType.Postgres ||
                                    databaseProvider == DatabaseProviderType.Oracle ||
                                    databaseProvider == DatabaseProviderType.SqlServer)
                            {
                                _ = propertyBuilder.HasPrecision(20);
                            }
                        },
                        OverridenColumnMappingAction = UInt64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = UInt64ColumnMappingType.AsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion<string>().HasMaxLength(20).IsUnicode(false); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    }
                ]),
            [typeof(float)] = new MappingRulesOf<float, FloatColumnMappingType>(
                noneValue: FloatColumnMappingType.None,
                defaultValue: FloatColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = FloatColumnMappingType.AsNumeric,
                        OverridenColumnMappingAction = FloatColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = FloatColumnMappingType.AsDoublePrecisionFloatingPoint,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion<double>(); },
                        OverridenColumnMappingAction = DoubleColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = FloatColumnMappingType.AsDecimal,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion<decimal>().HasPrecision(38, 18); },
                        OverridenColumnMappingAction = DecimalColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = FloatColumnMappingType.AsOACurrency,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new FloatToOACurrencyConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _int64MappingHints : null));
                        },
                        OverridenColumnMappingAction = Int64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = FloatColumnMappingType.AsScientificString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new FloatToStringInScientificNotationConverter(new(size: 16, unicode: false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = FloatColumnMappingType.AsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion<string>().HasMaxLength(180).IsUnicode(false); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    }
                ]),
            [typeof(double)] = new MappingRulesOf<double, DoubleColumnMappingType>(
                noneValue: DoubleColumnMappingType.None,
                defaultValue: DoubleColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = DoubleColumnMappingType.AsNumeric,
                        OverridenColumnMappingAction = DoubleColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DoubleColumnMappingType.AsSinglePrecisionFloatingPoint,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion<float>(); },
                        OverridenColumnMappingAction = FloatColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DoubleColumnMappingType.AsDecimal,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion<decimal>().HasPrecision(38, 18); },
                        OverridenColumnMappingAction = DecimalColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DoubleColumnMappingType.AsOACurrency,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new DoubleToOACurrencyConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _int64MappingHints : null));
                        },
                        OverridenColumnMappingAction = Int64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DoubleColumnMappingType.AsScientificString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new DoubleToStringInScientificNotationConverter(new(size: 32, unicode: false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DoubleColumnMappingType.AsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion<string>().HasMaxLength(768).IsUnicode(false); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    }
                ]),
            [typeof(decimal)] = new MappingRulesOf<decimal, DecimalColumnMappingType>(
                noneValue: DecimalColumnMappingType.None,
                defaultValue: DecimalColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = DecimalColumnMappingType.AsNumeric,
                        OverridenColumnMappingAction = DecimalColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DecimalColumnMappingType.AsSinglePrecisionFloatingPoint,
                        PropertyBuilderAction = (propertyBuilder, __) =>
                        {
                            _ = propertyBuilder.HasConversion<float>();

                            propertyBuilder.Metadata.SetPrecision(null);
                            propertyBuilder.Metadata.SetScale(null);
                        },
                        OverridenColumnMappingAction = FloatColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DecimalColumnMappingType.AsDoublePrecisionFloatingPoint,
                        PropertyBuilderAction = (propertyBuilder, __) =>
                        {
                            _ = propertyBuilder.HasConversion<double>();

                            propertyBuilder.Metadata.SetPrecision(null);
                            propertyBuilder.Metadata.SetScale(null);
                        },
                        OverridenColumnMappingAction = DoubleColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DecimalColumnMappingType.AsOACurrency,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new DecimalToOACurrencyConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _int64MappingHints : null));
                        },
                        OverridenColumnMappingAction = Int64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DecimalColumnMappingType.AsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion<string>().HasMaxLength(68).IsUnicode(false); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                ]),
            [typeof(DateOnly)] = new MappingRulesOf<DateOnly, DateOnlyColumnMappingType>(
                noneValue: DateOnlyColumnMappingType.None,
                defaultValue: DateOnlyColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = DateOnlyColumnMappingType.AsDate,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            if (databaseProvider == DatabaseProviderType.MySql ||
                                databaseProvider == DatabaseProviderType.Oracle)
                            {
                                _ = propertyBuilder.HasConversion(new DateOnlyToDateTimeConverter());
                            }
                        },
                        OverridenColumnMappingAction = DateOnlyColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateOnlyColumnMappingType.AsSerial,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new DateOnlyToSerialConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _actionIntHints : null));
                        },
                        OverridenColumnMappingAction = Int32ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateOnlyColumnMappingType.AsSerialString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new DateOnlyToSerialStringConverter(new(size: 8, unicode: false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateOnlyColumnMappingType.AsOADateTime,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new DateOnlyToOADateTimeConverter()); },
                        OverridenColumnMappingAction = DoubleColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateOnlyColumnMappingType.AsISOString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new DateOnlyToISOStringConverter(new(size: 10, unicode: false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateOnlyColumnMappingType.AsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion<string>().HasMaxLength(10).IsUnicode(false); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    }
                ]),
            [typeof(TimeOnly)] = new MappingRulesOf<TimeOnly, TimeOnlyColumnMappingType>(
                noneValue: TimeOnlyColumnMappingType.None,
                defaultValue: TimeOnlyColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = TimeOnlyColumnMappingType.AsTime,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            if (databaseProvider == DatabaseProviderType.MySql)
                            {
                                _ = propertyBuilder.HasConversion(new TimeOnlyToTimeSpanConverter(new(size: 6)));
                            }
                            else if (databaseProvider == DatabaseProviderType.Oracle)
                            {
                                _ = propertyBuilder.HasConversion(new TimeOnlyToTimeSpanConverter(new(size: 7)));
                            }
                            else if (databaseProvider == DatabaseProviderType.SqlServer)
                            {
                                _ = propertyBuilder.HasPrecision(7);
                            }
                        },
                        OverridenColumnMappingAction = TimeOnlyColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = TimeOnlyColumnMappingType.AsNanoseconds,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new TimeOnlyToNanosecondsConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _int64MappingHints : null));
                        },
                        OverridenColumnMappingAction = Int64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = TimeOnlyColumnMappingType.AsTicks,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new TimeOnlyToTicksConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _int64MappingHints : null));
                        },
                        OverridenColumnMappingAction = Int64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = TimeOnlyColumnMappingType.AsTicksString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new TimeOnlyToTicksStringConverter(new(size: 20, unicode: false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = TimeOnlyColumnMappingType.AsSerial,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new TimeOnlyToSerialConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _actionIntHints : null));
                        },
                        OverridenColumnMappingAction = Int32ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = TimeOnlyColumnMappingType.AsSerialString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new TimeOnlyToSerialStringConverter(new(size: 6, unicode: false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = TimeOnlyColumnMappingType.AsSerialWithNanoseconds,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new TimeOnlyToSerialWithNanosecondsConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _int64MappingHints : null));
                        },
                        OverridenColumnMappingAction = Int64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = TimeOnlyColumnMappingType.AsSerialWithNanosecondsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new TimeOnlyToSerialWithNanosecondsStringConverter(new(size: 15, unicode : false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = TimeOnlyColumnMappingType.AsISOString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new TimeOnlyToISOStringConverter(new(size: 18, unicode: false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = TimeOnlyColumnMappingType.AsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new TimeOnlyToStringConverter(new(size: 18, unicode:false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    }
                ]),
            [typeof(DateTime)] = new MappingRulesOf<DateTime, DateTimeColumnMappingType>(
                noneValue: DateTimeColumnMappingType.None,
                defaultValue: DateTimeColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = DateTimeColumnMappingType.AsUtcDateTime,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            ConverterMappingHints? hints = null;

                            if (databaseProvider == DatabaseProviderType.Sqlite)
                            {
                                hints = new(precision: 6);
                            }
                            else if (databaseProvider == DatabaseProviderType.Oracle ||
                                    databaseProvider == DatabaseProviderType.SqlServer)
                            {
                                hints = new(precision: 7);
                            }

                            _ = propertyBuilder.HasConversion(new DateTimeToUtcConverter(hints,
                                databaseProvider == DatabaseProviderType.Postgres ? DateTimeKind.Unspecified : null));
                        },
                        OverridenColumnMappingAction = DateTimeColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeColumnMappingType.AsLocalDateTime,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            ConverterMappingHints? hints = null;

                            if (databaseProvider == DatabaseProviderType.Sqlite)
                            {
                                hints = new(precision: 6);
                            }
                            else if (databaseProvider == DatabaseProviderType.Oracle ||
                                    databaseProvider == DatabaseProviderType.SqlServer)
                            {
                                hints = new(precision: 7);
                            }

                            _ = propertyBuilder.HasConversion(new DateTimeToLocalConverter(hints));
                        },
                        OverridenColumnMappingAction = DateTimeColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeColumnMappingType.AsUtcSerial,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new DateTimeUtcToSerialConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _int64MappingHints : null));
                        },
                        OverridenColumnMappingAction = Int64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeColumnMappingType.AsUtcSerialWithMilliseconds,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new DateTimeUtcToSerialWithMillisecondsConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _int64MappingHints : null));
                        },
                        OverridenColumnMappingAction = Int64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeColumnMappingType.AsUtcSerialString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new DateTimeUtcToSerialStringConverter(new (size: 14, unicode:false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeColumnMappingType.AsUtcSerialWithMillisecondsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new DateTimeUtcToSerialStringWithMillisecondsConverter(new (size: 17, unicode:false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeColumnMappingType.AsUnixSeconds,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new DateTimeUtcToUnixSecondsConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _int64MappingHints : null));
                        },
                        OverridenColumnMappingAction = Int64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeColumnMappingType.AsUnixMilliseconds,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new DateTimeUtcToUnixMillisecondsConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _int64MappingHints : null));
                        },
                        OverridenColumnMappingAction = Int64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeColumnMappingType.AsOADateTime,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new DateTimeUtcToOADateTimeConverter()); },
                        OverridenColumnMappingAction = DoubleColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeColumnMappingType.AsUtcString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new DateTimeUtcToStringConverter(new(size: 33, unicode:false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeColumnMappingType.AsLocalString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new DateTimeLocalToStringConverter(new(size: 33, unicode:false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeColumnMappingType.AsISOString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new DateTimeToISOStringConverter(new(size: 30, unicode:false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    }
                ]),
            [typeof(DateTimeOffset)] = new MappingRulesOf<DateTimeOffset, DateTimeOffsetColumnMappingType>(
                noneValue: DateTimeOffsetColumnMappingType.None,
                defaultValue: DateTimeOffsetColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = DateTimeOffsetColumnMappingType.AsDateTimeOffset,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            if (databaseProvider == DatabaseProviderType.Sqlite ||
                                databaseProvider == DatabaseProviderType.MySql)
                            {
                                _ = propertyBuilder.HasPrecision(6);
                                _ = propertyBuilder.HasConversion(new DateTimeOffsetToUtcDateTimeConverter(
                                    (databaseProvider == DatabaseProviderType.MySql) ? new(precision: 6) : null));
                            }
                            else if (databaseProvider == DatabaseProviderType.Postgres)
                            {
                                _ = propertyBuilder.HasConversion(new DateTimeOffsetToUtcConverter());
                            }
                            else
                            {
                                _ = propertyBuilder.HasConversion(null as Type);

                                if (databaseProvider == DatabaseProviderType.Oracle ||
                                    databaseProvider == DatabaseProviderType.SqlServer)
                                {
                                    _ = propertyBuilder.HasPrecision(7);
                                }
                            }
                        },
                        OverridenColumnMappingAction = DateTimeOffsetColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeOffsetColumnMappingType.AsUtcDateTime,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new DateTimeOffsetToUtcDateTimeConverter(
                                (databaseProvider == DatabaseProviderType.MySql) ? new(precision: 6) :
                                (databaseProvider == DatabaseProviderType.Oracle ||
                                databaseProvider == DatabaseProviderType.SqlServer) ? new(precision: 7) : null,
                                databaseProvider == DatabaseProviderType.Postgres ? DateTimeKind.Unspecified : null));
                        },
                        OverridenColumnMappingAction = DateTimeColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeOffsetColumnMappingType.AsLocalDateTime,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new DateTimeOffsetToLocalDateTimeConverter(
                                (databaseProvider == DatabaseProviderType.MySql) ? new(precision: 6) :
                                (databaseProvider == DatabaseProviderType.Oracle ||
                                databaseProvider == DatabaseProviderType.SqlServer) ? new(precision: 7) : null));
                        },
                        OverridenColumnMappingAction = DateTimeColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeOffsetColumnMappingType.AsUtcSerial,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new DateTimeOffsetUtcToSerialConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _int64MappingHints : null));
                        },
                        OverridenColumnMappingAction = Int64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeOffsetColumnMappingType.AsUtcSerialWithMilliseconds,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new DateTimeOffsetUtcToSerialWithMillisecondsConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _int64MappingHints : null));
                        },
                        OverridenColumnMappingAction = Int64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeOffsetColumnMappingType.AsUtcSerialString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new DateTimeOffsetUtcToSerialStringConverter(new(size: 14, unicode: false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeOffsetColumnMappingType.AsUtcSerialWithMillisecondsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new DateTimeOffsetUtcToSerialWithMillisecondsStringConverter(new(size: 17, unicode: false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeOffsetColumnMappingType.AsUnixSeconds,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new DateTimeOffsetToUnixSecondsConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _int64MappingHints : null));
                        },
                        OverridenColumnMappingAction = Int64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeOffsetColumnMappingType.AsUnixMilliseconds,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new DateTimeOffsetToUnixMillisecondsConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _int64MappingHints : null));
                        },
                        OverridenColumnMappingAction = Int64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeOffsetColumnMappingType.AsOADateTime,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new DateTimeOffsetUtcToOADateTimeOffsetConverter()); },
                        OverridenColumnMappingAction = DoubleColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeOffsetColumnMappingType.AsUtcString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new DateTimeOffsetUtcToStringConverter(new(size: 33, unicode: false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeOffsetColumnMappingType.AsLocalString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new DateTimeOffsetLocalToStringConverter(new(size: 33, unicode: false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeOffsetColumnMappingType.AsISOString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new DateTimeOffsetToISOStringConverter(new(size: 33, unicode: false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = DateTimeOffsetColumnMappingType.AsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new DateTimeOffsetToStringConverter(new(size: 34, unicode: false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    }
                ]),
            [typeof(TimeSpan)] = new MappingRulesOf<TimeSpan, TimeSpanColumnMappingType>(
                noneValue: TimeSpanColumnMappingType.None,
                defaultValue: TimeSpanColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = TimeSpanColumnMappingType.AsTimeSpan,
                        OverridenColumnMappingAction = TimeSpanColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = TimeSpanColumnMappingType.AsNanoseconds,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new TimeSpanToNanosecondsConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _int64MappingHints : null));
                        },
                        OverridenColumnMappingAction = Int64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = TimeSpanColumnMappingType.AsNanosecondsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new TimeSpanToNanosecondsStringConverter(new(size: 26, unicode:false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = TimeSpanColumnMappingType.AsTicks,
                        PropertyBuilderAction = (propertyBuilder, databaseProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new TimeSpanToTicksConverter(
                                (databaseProvider == DatabaseProviderType.Oracle) ? _int64MappingHints : null));
                        },
                        OverridenColumnMappingAction = Int64ColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = TimeSpanColumnMappingType.AsTicksString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new TimeSpanToTicksStringConverter(new(size: 24, unicode:false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = TimeSpanColumnMappingType.AsString,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new TimeSpanToStringConverter(new(size: 26, unicode:false))); },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                ]),
            [typeof(Guid)] = new MappingRulesOf<Guid, GuidColumnMappingType>(
                noneValue: GuidColumnMappingType.None,
                defaultValue: GuidColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = GuidColumnMappingType.AsUuid,
                        PropertyBuilderAction = (propertyBuilder, databaserProvider) =>
                        {
                            if (databaserProvider == DatabaseProviderType.Sqlite)
                            {
                                _ = propertyBuilder.HasConversion(new GuidToCompactStringConverter(new(size: 32, unicode: false)));
                            }
                            else if (databaserProvider == DatabaseProviderType.MySql ||
                                    databaserProvider == DatabaseProviderType.Oracle)
                            {
                                _ = propertyBuilder.HasConversion(new GuidToBigEndianByteArrayConverter(new(size: 16))).IsFixedLength();
                            }
                            else
                            {
                                _ = propertyBuilder.HasConversion(null as Type);
                            }
                        },
                        OverridenColumnMappingAction = GuidColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = GuidColumnMappingType.AsBigEndianByteArray,
                        PropertyBuilderAction = (propertyBuilder, databaserProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new GuidToBigEndianByteArrayConverter());

                            if (databaserProvider == DatabaseProviderType.MySql ||
                                databaserProvider == DatabaseProviderType.Oracle ||
                                databaserProvider == DatabaseProviderType.SqlServer)
                            {
                                _ = propertyBuilder.IsFixedLength().HasMaxLength(16);
                            }
                        },
                        OverridenColumnMappingAction = ByteArrayColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = GuidColumnMappingType.AsCompactString,
                        PropertyBuilderAction = (propertyBuilder, databaserProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new GuidToCompactStringConverter(new(size: 32, unicode: false)));

                            if (databaserProvider == DatabaseProviderType.MySql)
                            {
                                _ = propertyBuilder.IsFixedLength(false);
                            }
                        },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = GuidColumnMappingType.AsString,
                        PropertyBuilderAction = (propertyBuilder, databaserProvider) =>
                        {
                            _ = propertyBuilder.HasConversion(new GuidToStringConverter(new(size: 36, unicode: false)));

                            if (databaserProvider == DatabaseProviderType.MySql)
                            {
                                _ = propertyBuilder.IsFixedLength(false);
                            }
                        },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    }
                ]),
            [typeof(string)] = new MappingRulesOf<string, StringColumnMappingType>(
                noneValue: StringColumnMappingType.None,
                defaultValue: StringColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = StringColumnMappingType.AsStringBlob,
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = StringColumnMappingType.AsASCIIBinaryBlob,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new StringToASCIIBytesConverter()); },
                        OverridenColumnMappingAction = ByteArrayColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = StringColumnMappingType.AsUTF8BinaryBlob,
                        PropertyBuilderAction = (propertyBuilder, __) => { _ = propertyBuilder.HasConversion(new StringToUTF8BytesConverter()); },
                        OverridenColumnMappingAction = ByteArrayColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = StringColumnMappingType.AsString,
                        OverridenColumnMappingAction = StringColumnMappingAction
                    },
                ]),
            [typeof(char[])] = new MappingRulesOf<char[], CharArrayColumnMappingType>(
                noneValue: CharArrayColumnMappingType.None,
                defaultValue: CharArrayColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = CharArrayColumnMappingType.AsStringBlob,
                        PropertyBuilderAction = (propertyBuilder, __) =>
                        {
                            _ = propertyBuilder.HasConversion(new CharArrayToStringConverter());

                            propertyBuilder.Metadata.SetValueComparer(ValueComparer.CreateDefault<char[]>(favorStructuralComparisons: true));
                        },
                        OverridenColumnMappingAction = CharArrayColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = CharArrayColumnMappingType.AsASCIIBinaryBlob,
                        PropertyBuilderAction = (propertyBuilder, __) =>
                        {
                            _ = propertyBuilder.HasConversion(new CharArrayToASCIIBytesConverter());

                            propertyBuilder.Metadata.SetValueComparer(ValueComparer.CreateDefault<char[]>(favorStructuralComparisons: true));
                        },
                        OverridenColumnMappingAction = ByteArrayColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = CharArrayColumnMappingType.AsUTF8BinaryBlob,
                        PropertyBuilderAction = (propertyBuilder, __) =>
                        {
                            _ = propertyBuilder.HasConversion(new CharArrayToUTF8BytesConverter());

                            propertyBuilder.Metadata.SetValueComparer(ValueComparer.CreateDefault<char[]>(favorStructuralComparisons: true));
                        },
                        OverridenColumnMappingAction = ByteArrayColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = CharArrayColumnMappingType.AsString,
                        PropertyBuilderAction = (propertyBuilder, __) =>
                        {
                            _ = propertyBuilder.HasConversion(new CharArrayToStringConverter());

                            propertyBuilder.Metadata.SetValueComparer(ValueComparer.CreateDefault<char[]>(favorStructuralComparisons: true));
                        },
                        OverridenColumnMappingAction = CharArrayColumnMappingAction
                    },
                ]),
            [typeof(byte[])] = new MappingRulesOf<byte[], ByteArrayColumnMappingType>(
                noneValue: ByteArrayColumnMappingType.None,
                defaultValue: ByteArrayColumnMappingType.Default,
                mappingRules: [
                    new()
                    {
                        ColumnMappingType = ByteArrayColumnMappingType.AsBlob,
                        OverridenColumnMappingAction = ByteArrayColumnMappingAction
                    },
                    new()
                    {
                        ColumnMappingType = ByteArrayColumnMappingType.AsBase64String,
                        PropertyBuilderAction = (propertyBuilder, __) =>
                        {
                            _ = propertyBuilder.HasConversion(new BytesToStringConverter(new(unicode: false)));

                            propertyBuilder.Metadata.SetValueComparer(ValueComparer.CreateDefault<byte[]>(favorStructuralComparisons: true));
                        },
                        OverridenColumnMappingAction = StringColumnMappingAction
                    }
                ]),
        };

        return rules;
    });

    public static void CheckRules()
    {
        var rules = Rules.Value.Values;

        foreach (var rule in rules)
        {
            var defaultColumnMappingActionName = ColumnMappingActionOfType(rule.ClrType).ToString();
            var defaultTypeAsNumber = Convert.ToInt64(rule.Default, CultureInfo.InvariantCulture);

            Console.WriteLine($"\r\n\r\nFor CLR type {rule.ClrType}:\r\n");

            rule.ForEachMappingType((mappingType, mapping) =>
            {
                var mappingTypeAsNumber = Convert.ToInt64(mappingType, CultureInfo.InvariantCulture);
                var mappedColumnType = mapping.OverridenColumnMappingAction?.ToString();

                var overridenMappingTypeMethod = mapping.OverridenColumnMappingAction?.Method?.DeclaringType?.Name +
                    "." + mapping.OverridenColumnMappingAction?.Method?.Name;

                Console.WriteLine($"- MappingType: [{mappingTypeAsNumber}] {mappingType}{(defaultTypeAsNumber == mappingTypeAsNumber ? " (** Default **)" : "")} => {overridenMappingTypeMethod}");
            });
        }
    }

    private static (int? columnSize, int? columnPrecision, int? columnScale, bool? isUnicode, bool? isFixedLength, Type? propertyModelType, Type? propertyDatabaseType)
        ColumnAttributesOf<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType _) where TPropertyBuilder : PropertyBuilder
    {
        var propertyType = propertyBuilder.Metadata.ClrType;
        var propertyModelType = propertyType.NonNullableType();

        var valueConverter = propertyBuilder.Metadata.GetValueConverter();

        var propertyModelTypeAfterConversion = valueConverter != null ? valueConverter.ProviderClrType : (propertyBuilder.Metadata.GetProviderClrType() ?? propertyModelType);
        var propertyDatabaseType = propertyModelTypeAfterConversion.NonNullableType();

        var columnSize = propertyBuilder.Metadata.GetMaxLength() ?? valueConverter?.MappingHints?.Size ?? null;
        var columnPrecision = propertyBuilder.Metadata.GetPrecision() ?? valueConverter?.MappingHints?.Precision ?? null;
        var columnScale = propertyBuilder.Metadata.GetScale() ?? valueConverter?.MappingHints?.Scale ?? null;
        var isUnicode = propertyBuilder.Metadata.IsUnicode() ?? valueConverter?.MappingHints?.IsUnicode ?? null;
        var isFixedLength = propertyBuilder.Metadata.IsFixedLength() ?? null;

        return (columnSize, columnPrecision, columnScale, isUnicode, isFixedLength, propertyModelType, propertyDatabaseType);
    }

    private static (string? columnType, string? collation, string? characterSet) BooleanColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, _, _, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "INTEGER",
            DatabaseProviderType.MySql => "BIT(1)",
            DatabaseProviderType.Postgres => "BOOLEAN",
            DatabaseProviderType.Oracle => "NUMBER(1)",
            DatabaseProviderType.SqlServer => "BIT",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static (string? columnType, string? collation, string? characterSet) Int8ColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, precision, _, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        precision ??= 3;

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "INTEGER",
            DatabaseProviderType.MySql => "TINYINT",
            DatabaseProviderType.Postgres => "SMALLINT",
            DatabaseProviderType.Oracle => $"NUMBER({precision})",
            DatabaseProviderType.SqlServer => "TINYINT",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static (string? columnType, string? collation, string? characterSet) UInt8ColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, precision, _, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        precision ??= 3;

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "INTEGER",
            DatabaseProviderType.MySql => "TINYINT UNSIGNED",
            DatabaseProviderType.Postgres => "SMALLINT",
            DatabaseProviderType.Oracle => $"NUMBER({precision})",
            DatabaseProviderType.SqlServer => "SMALLINT",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static (string? columnType, string? collation, string? characterSet) Int16ColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, precision, _, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        precision ??= 5;

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "INTEGER",
            DatabaseProviderType.MySql => "SMALLINT",
            DatabaseProviderType.Postgres => "SMALLINT",
            DatabaseProviderType.Oracle => $"NUMBER({precision})",
            DatabaseProviderType.SqlServer => "SMALLINT",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static (string? columnType, string? collation, string? characterSet) UInt16ColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, precision, _, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        precision ??= 5;

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "INTEGER",
            DatabaseProviderType.MySql => "SMALLINT UNSIGNED",
            DatabaseProviderType.Postgres => "INTEGER",
            DatabaseProviderType.Oracle => $"NUMBER({precision})",
            DatabaseProviderType.SqlServer => "INT",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static readonly ConverterMappingHints _actionIntHints = new(precision: 10);

    private static (string? columnType, string? collation, string? characterSet) Int32ColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, precision, _, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        precision ??= 10;

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "INTEGER",
            DatabaseProviderType.MySql => "INT",
            DatabaseProviderType.Postgres => "INTEGER",
            DatabaseProviderType.Oracle => $"NUMBER({precision})",
            DatabaseProviderType.SqlServer => "INT",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static (string? columnType, string? collation, string? characterSet) UInt32ColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, precision, _, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        precision ??= 10;

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "INTEGER",
            DatabaseProviderType.MySql => "INT UNSIGNED",
            DatabaseProviderType.Postgres => "BIGINT",
            DatabaseProviderType.Oracle => $"NUMBER({precision})",
            DatabaseProviderType.SqlServer => "BIGINT",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static readonly ConverterMappingHints _int64MappingHints = new(precision: 19, scale: 0);

    private static (string? columnType, string? collation, string? characterSet) Int64ColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, precision, _, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        precision ??= 19;

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "INTEGER",
            DatabaseProviderType.MySql => "BIGINT",
            DatabaseProviderType.Postgres => "BIGINT",
            DatabaseProviderType.Oracle => $"NUMBER({precision})",
            DatabaseProviderType.SqlServer => "BIGINT",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static (string? columnType, string? collation, string? characterSet) UInt64ColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, precision, _, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        precision ??= 20;

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "INTEGER",
            DatabaseProviderType.MySql => "BIGINT UNSIGNED",
            DatabaseProviderType.Postgres => $"NUMERIC({precision})",
            DatabaseProviderType.Oracle => $"NUMBER({precision})",
            DatabaseProviderType.SqlServer => $"DECIMAL({precision})",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static (string? columnType, string? collation, string? characterSet) FloatColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, _, _, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "REAL",
            DatabaseProviderType.MySql => "FLOAT",
            DatabaseProviderType.Postgres => "REAL",
            DatabaseProviderType.Oracle => "BINARY_FLOAT",
            DatabaseProviderType.SqlServer => "REAL",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static (string? columnType, string? collation, string? characterSet) DoubleColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, _, _, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "REAL",
            DatabaseProviderType.MySql => "DOUBLE",
            DatabaseProviderType.Postgres => "DOUBLE PRECISION",
            DatabaseProviderType.Oracle => "BINARY_DOUBLE",
            DatabaseProviderType.SqlServer => "FLOAT",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static (string? columnType, string? collation, string? characterSet) DecimalColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, precision, scale, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        scale ??= (precision == null ? 18 : 0);
        precision ??= 38;

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "NUMERIC",
            DatabaseProviderType.MySql => $"DECIMAL({precision}, {scale})",
            DatabaseProviderType.Postgres => $"NUMERIC({precision}, {scale})",
            DatabaseProviderType.Oracle => $"NUMBER({precision}, {scale})",
            DatabaseProviderType.SqlServer => $"DECIMAL({precision}, {scale})",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static (string? columnType, string? collation, string? characterSet) DateOnlyColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, _, _, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "TEXT",
            DatabaseProviderType.MySql => "DATE",
            DatabaseProviderType.Postgres => "DATE",
            DatabaseProviderType.Oracle => "DATE",
            DatabaseProviderType.SqlServer => "DATE",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static int? FixUpFractionalSeconds(int? precision, DatabaseProviderType databaseProvider)
    {
        int? maxPrecision = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => null,
            DatabaseProviderType.MySql => MySqlTimeMaxFractionalSecondDigits,
            DatabaseProviderType.Postgres => PostgresTimeMaxFractionalSecondDigits,
            DatabaseProviderType.Oracle => OracleTimeMaxFractionalSecondDigits,
            DatabaseProviderType.SqlServer => SqlServerTimeMaxFractionalSecondDigits,
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        precision ??= maxPrecision;

        if (precision != null)
        {
            if (precision < 0)
            {
                precision = 0;
            }
            else if ((maxPrecision != null) && (precision > maxPrecision))
            {
                precision = maxPrecision;
            }
        }

        return precision;
    }

    private static (string? columnType, string? collation, string? characterSet) TimeOnlyColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, precision, _, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        precision = FixUpFractionalSeconds(precision, databaseProvider);

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "TEXT",
            DatabaseProviderType.MySql => $"TIME({(precision)})",
            DatabaseProviderType.Postgres => $"TIME({(precision)})",
            DatabaseProviderType.Oracle => $"INTERVAL DAY(0) TO SECOND({(precision)})",
            DatabaseProviderType.SqlServer => $"TIME({(precision)})",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static (string? columnType, string? collation, string? characterSet) DateTimeColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, precision, _, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        precision = FixUpFractionalSeconds(precision, databaseProvider);

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "TEXT",
            DatabaseProviderType.MySql => $"DATETIME({(precision)})",
            DatabaseProviderType.Postgres => $"TIMESTAMP({(precision)})",
            DatabaseProviderType.Oracle => $"TIMESTAMP({(precision)})",
            DatabaseProviderType.SqlServer => $"DATETIME2({(precision)})",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static (string? columnType, string? collation, string? characterSet) DateTimeOffsetColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, precision, _, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        precision = FixUpFractionalSeconds(precision, databaseProvider);

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "TEXT",
            DatabaseProviderType.MySql => $"DATETIME({(precision)})",
            DatabaseProviderType.Postgres => $"TIMESTAMP({(precision)}) WITH TIME ZONE",
            DatabaseProviderType.Oracle => $"TIMESTAMP({(precision)}) WITH TIME ZONE",
            DatabaseProviderType.SqlServer => $"DATETIMEOFFSET({(precision)})",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static (string? columnType, string? collation, string? characterSet) TimeSpanColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, _, _, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "INTEGER",
            DatabaseProviderType.MySql => "BIGINT",
            DatabaseProviderType.Postgres => "BIGINT",
            DatabaseProviderType.Oracle => "NUMBER(19)",
            DatabaseProviderType.SqlServer => "BIGINT",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static (string? columnType, string? collation, string? characterSet) GuidColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (_, _, _, _, _, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        var columnType = databaseProvider switch
        {
            DatabaseProviderType.Unknown => null,
            DatabaseProviderType.InMemory => null,
            DatabaseProviderType.Sqlite => "TEXT",
            DatabaseProviderType.MySql => $"BINARY(16)",
            DatabaseProviderType.Postgres => "UUID",
            DatabaseProviderType.Oracle => $"RAW(16)",
            DatabaseProviderType.SqlServer => $"DATABASEIDENTIFIER",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider."),
        };

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static (string? columnType, string? collation, string? characterSet) ByteArrayColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        var (columnSize, _, _, _, isFixedLength, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        if (columnSize == -1)
        {
            columnSize = null;
        }

        isFixedLength ??= false;

        string? columnType = null;

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
                columnType = "BLOB";

                break;
            }
        case DatabaseProviderType.MySql:
            {
                if (columnSize == null)
                {
                    columnType = "LONGBLOB";
                }
                else
                {
                    if (columnSize <= 255)
                    {
                        columnType = $"{(isFixedLength.Value ? "" : "VAR")}BINARY({columnSize})"; /* "TINYBLOB" */
                    }
                    else if (columnSize <= 65535)
                    {
                        columnType = $"{(isFixedLength.Value ? "" : "VAR")}BINARY({columnSize})"; /* "BLOB" */
                    }
                    else if (columnSize <= 16777216)
                    {
                        columnType = "MEDIUMBLOB";
                    }
                    else
                    {
                        columnType = "LONGBLOB";
                    }
                }

                break;
            }
        case DatabaseProviderType.Postgres:
            {
                if (columnSize == null)
                {
                    columnType = "BYTEA";
                }
                else
                {
                    if (columnSize <= 64000)
                    {
                        columnType = $"BYTEA CHECK(octet_length(\"{propertyBuilder.Metadata.GetColumnName()}\") <= {columnSize})";
                    }
                    else
                    {
                        columnType = "BYTEA";
                    }
                }

                break;
            }
        case DatabaseProviderType.Oracle:
            {
                if (columnSize == null)
                {
                    columnType = "BLOB";
                }
                else
                {
                    if (columnSize <= 2000)
                    {
                        columnType = $"RAW({columnSize})";
                    }
                    else
                    {
                        columnType = "BLOB";
                    }
                }

                break;
            }
        case DatabaseProviderType.SqlServer:
            {
                if (columnSize == null)
                {
                    columnType = $"VARBINARY(MAX)";
                }
                else
                {
                    if (columnSize <= 8000)
                    {
                        columnType = $"VARBINARY({columnSize})";
                    }
                    else
                    {
                        columnType = $"VARBINARY(MAX)";
                    }
                }

                break;
            }
        default:
            {
                throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider.");
            }
        }

        string? collation = null;
        string? characterSet = null;

        return (columnType, collation, characterSet);
    }

    private static (string? columnType, string? collation, string? characterSet) CharArrayColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder => StringColumnMappingAction(propertyBuilder, databaseProvider);

    private static (string? columnType, string? collation, string? characterSet) StringColumnMappingAction<TPropertyBuilder>(TPropertyBuilder propertyBuilder, DatabaseProviderType databaseProvider)
        where TPropertyBuilder : PropertyBuilder
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder, nameof(propertyBuilder));

        var (columnSize, _, _, isUnicode, isFixedLength, _, _) = ColumnAttributesOf(propertyBuilder, databaseProvider);

        if (columnSize == -1)
        {
            columnSize = null;
        }

        isFixedLength ??= false;
        isUnicode ??= true;

        if (isUnicode.Value)
        {
            var hasDataProtectionConversion = propertyBuilder.HasDataProtectionConversion();

            if (hasDataProtectionConversion)
            {
                isUnicode = false;
            }
        }

        string? columnType = null;
        string? collation = null;
        string? characterSet = null;

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
                columnType = "TEXT";

                break;
            }
        case DatabaseProviderType.MySql:
            {
                if (isUnicode.Value)
                {
                    collation = MySqlUnicodeCollation;
                    characterSet = MySqlUnicodeCharacterSet;
                }

                if (columnSize == null)
                {
                    columnType = $"LONGTEXT";
                }
                else
                {
                    if (columnSize <= 255)
                    {
                        columnType = $"{(isFixedLength.Value ? "" : "VAR")}CHAR({columnSize})"; /* "TINYTEXT" */
                    }
                    else if (columnSize <= 65535)
                    {
                        columnType = $"{(isFixedLength.Value ? "" : "VAR")}CHAR({columnSize})"; /* "TEXT" */
                    }
                    else if (columnSize <= 16777216)
                    {
                        columnType = $"MEDIUMTEXT";
                    }
                    else
                    {
                        columnType = $"LONGTEXT";
                    }
                }

                break;
            }
        case DatabaseProviderType.Postgres:
            {
                if (isUnicode.Value)
                {
                    collation = PostgresUnicodeCollation;
                }

                if (columnSize == null)
                {
                    columnType = "TEXT";
                }
                else
                {
                    if (columnSize <= 10485760)
                    {
                        columnType = $"CHARACTER{(isFixedLength.Value ? "" : " VARYING")}({columnSize})"; /* "TEXT" */
                    }
                    else
                    {
                        columnType = "TEXT";
                    }
                }

                break;
            }
        case DatabaseProviderType.Oracle:
            {
                if (isUnicode.Value)
                {
                    collation = OracleUnicodeCollation;
                }

                if (columnSize == null)
                {
                    columnType = $"{(isUnicode.Value ? "N" : "")}CLOB";
                }
                else
                {
                    if (columnSize <= 4000)
                    {
                        columnType = $"{(isUnicode.Value ? "N" : "")}{(isFixedLength.Value ? "" : "VAR")}CHAR{(isFixedLength.Value ? "" : "2")}({columnSize})";
                    }
                    else
                    {
                        columnType = $"{(isUnicode.Value ? "N" : "")}CLOB";
                    }
                }

                break;
            }
        case DatabaseProviderType.SqlServer:
            {
                if (isUnicode.Value)
                {
                    collation = SqlServerUnicodeCollation;
                }

                if (columnSize == null)
                {
                    columnType = $"{(isUnicode.Value ? "N" : "")}VARCHAR(MAX)";
                }
                else
                {
                    if (columnSize <= (isUnicode.Value ? 4000 : 8000))
                    {
                        columnType = $"{(isUnicode.Value ? "N" : "")}VARCHAR({columnSize})";
                    }
                    else
                    {
                        columnType = $"{(isUnicode.Value ? "N" : "")}VARCHAR(MAX)";
                    }
                }

                break;
            }
        default:
            {
                throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider.");
            }
        }

        return (columnType, collation, characterSet);
    }

    public static Func<PropertyBuilder, DatabaseProviderType, (string? columnType, string? collation, string? characterSet)> ColumnMappingActionOf<TDatabasePropertyType>()
    {
        return ColumnMappingActionOfType(typeof(TDatabasePropertyType));
    }

    public static Func<PropertyBuilder, DatabaseProviderType, (string? columnType, string? collation, string? characterSet)> ColumnMappingActionOfType(Type databasePropertyType)
    {
        if (databasePropertyType == typeof(bool))
        {
            return BooleanColumnMappingAction;
        }
        else if (databasePropertyType == typeof(sbyte))
        {
            return Int8ColumnMappingAction;
        }
        else if (databasePropertyType == typeof(byte))
        {
            return UInt8ColumnMappingAction;
        }
        else if (databasePropertyType == typeof(short))
        {
            return Int16ColumnMappingAction;
        }
        else if (databasePropertyType == typeof(ushort))
        {
            return UInt16ColumnMappingAction;
        }
        else if (databasePropertyType == typeof(int))
        {
            return Int32ColumnMappingAction;
        }
        else if (databasePropertyType == typeof(uint))
        {
            return UInt32ColumnMappingAction;
        }
        else if (databasePropertyType == typeof(long))
        {
            return Int64ColumnMappingAction;
        }
        else if (databasePropertyType == typeof(ulong))
        {
            return UInt64ColumnMappingAction;
        }
        else if (databasePropertyType == typeof(float))
        {
            return FloatColumnMappingAction;
        }
        else if (databasePropertyType == typeof(double))
        {
            return DoubleColumnMappingAction;
        }
        else if (databasePropertyType == typeof(decimal))
        {
            return DecimalColumnMappingAction;
        }
        else if (databasePropertyType == typeof(DateOnly))
        {
            return DateOnlyColumnMappingAction;
        }
        else if (databasePropertyType == typeof(TimeOnly))
        {
            return TimeOnlyColumnMappingAction;
        }
        else if (databasePropertyType == typeof(DateTime))
        {
            return DateTimeColumnMappingAction;
        }
        else if (databasePropertyType == typeof(DateTimeOffset))
        {
            return DateTimeOffsetColumnMappingAction;
        }
        else if (databasePropertyType == typeof(TimeSpan))
        {
            return TimeSpanColumnMappingAction;
        }
        else if (databasePropertyType == typeof(Guid))
        {
            return GuidColumnMappingAction;
        }
        else if (databasePropertyType == typeof(string))
        {
            return StringColumnMappingAction;
        }
        else if (databasePropertyType == typeof(char[]))
        {
            return CharArrayColumnMappingAction;
        }
        else if (databasePropertyType == typeof(byte[]))
        {
            return ByteArrayColumnMappingAction;
        }

        throw new ArgumentOutOfRangeException(nameof(databasePropertyType), databasePropertyType, $"Unsupported database provider CLR type {databasePropertyType}.");
    }
}
