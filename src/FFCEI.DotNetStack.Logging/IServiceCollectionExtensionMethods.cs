using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Logging.Configurations;
using FFCEI.DotNetStack.Logging.Serilog;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Redaction;
using Serilog.Sinks.OpenTelemetry;

namespace FFCEI.DotNetStack.Logging;

public static class IServiceCollectionExtensionMethods
{
    public static IServiceCollection ReplicateStandardLoggerFiltersToProvider(this IServiceCollection services, Type loggerProviderType)
    {
        ArgumentNullException.ThrowIfNull(loggerProviderType, nameof(loggerProviderType));

        if (!loggerProviderType.Implements<ILoggerProvider>())
        {
            throw new ArgumentException($"The type {loggerProviderType} does not implement ILoggerProvider interface", nameof(loggerProviderType));
        }

        var providerTypeFullName = loggerProviderType.FullName;

        return services.Configure<LoggerFilterOptions>(options =>
        {
            var rulesToReplicate = options.Rules.Where(rule => rule.ProviderName == null).ToList();

            foreach (var rule in rulesToReplicate)
            {
                if (options.Rules.Any(existingRule => existingRule.ProviderName == providerTypeFullName &&
                    existingRule.CategoryName == rule.CategoryName && existingRule.LogLevel == rule.LogLevel &&
                    existingRule.Filter == rule.Filter))
                {
                    continue;
                }

                options.Rules.Add(new LoggerFilterRule(providerTypeFullName, rule.CategoryName, rule.LogLevel, rule.Filter));
            }
        });
    }

    public static IServiceCollection ReplicateStandardLoggerFiltersToProvider<TProvider>(this IServiceCollection services) where TProvider : ILoggerProvider
        => services.ReplicateStandardLoggerFiltersToProvider(typeof(TProvider));

    public static IServiceCollection ReplicateStandardLoggerFilters(this IServiceCollection services)
    {
        var loggerProviderTypeServices = services
            .Where(service => service.ServiceType.Implements<ILoggerProvider>() ||
                (service.ImplementationType != null && service.ImplementationType.Implements<ILoggerProvider>()))
            .ToList();

        foreach (var loggerProviderTypeService in loggerProviderTypeServices)
        {
            var implementationType = loggerProviderTypeService.ImplementationType;
            var implementationFactory = loggerProviderTypeService.ImplementationFactory;

            if ((implementationType == null) && (implementationFactory != null))
            {
                var targetType = implementationFactory.Method.DeclaringType;
                var assemblyName = targetType?.Assembly.GetName().Name;

                if ((assemblyName == "Serilog.Extensions.Logging") &&
                    (targetType?.FullName?.StartsWith("Serilog.SerilogLoggingBuilderExtensions", StringComparison.InvariantCulture) ?? false))
                {
                    implementationType = typeof(SerilogLoggerProvider);
                }
            }

            if (implementationType != null)
            {
                _ = services.ReplicateStandardLoggerFiltersToProvider(implementationType);
            }
        }

        return services;
    }

    public static IServiceCollection ReplicateLoggerFiltersToSerilog(this IServiceCollection services)
        => services.ReplicateStandardLoggerFiltersToProvider<SerilogLoggerProvider>();

    public static IServiceCollection AddSerilogLogProvider(this IServiceCollection services,
        LoggerConfiguration? serilogConfiguration = null,
        Microsoft.Extensions.Logging.ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;

        if (serilogConfiguration == null)
        {
            serilogConfiguration = SerilogLogger.CreateSerilogConfiguration();

            _ = serilogConfiguration.AddConsoleLogging();

            if (System.Diagnostics.Debugger.IsAttached)
            {
                _ = serilogConfiguration.AddDebugLogging();
            }
        }

        _ = services.AddLogging(loggingBuilder =>
        {
            _ = loggingBuilder
                .ClearProviders();

            _ = loggingBuilder.Services.AddSingleton<ILoggerProvider>(serviceProvider =>
                {
                    var redactorProvider = serviceProvider.GetService<IRedactorProvider>();

                    if (redactorProvider != null)
                    {
                        _ = serilogConfiguration.Destructure.WithRedaction(redactorProvider);
                    }

                    Log.Logger = serilogConfiguration.CreateLogger();

                    return new SerilogLoggerProvider(Log.Logger, dispose: true);
                });
        })
        .ReplicateLoggerFiltersToSerilog();

        LoggingLog.SerilogConfigured(logger);

        return services;
    }

    public static IServiceCollection AddLoggerFactoryUsingSerilog(this IServiceCollection services,
        LoggerFactoryConfiguration configuration,
        Action<LoggerConfiguration>? builder = null,
        Microsoft.Extensions.Logging.ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        logger ??= NullLogger.Instance;

        var serilogLoggerConfiguration = SerilogLogger.CreateSerilogConfiguration(configuration.MinimumLogLevel.ToLogEventLevel());

        LoggingLog.CreatingSerilogConfiguration(logger);

        if (configuration.Console != null)
        {
            _ = serilogLoggerConfiguration.AddConsoleLogging(
                configuration.Console.JsonLogging,
                configuration.Console.MinimumLogLevel.ToLogEventLevel());

            LoggingLog.AddSerilogConsoleLogging(logger);
        }

        if (configuration.Debugger != null)
        {
            _ = serilogLoggerConfiguration.AddDebugLogging(
                configuration.Debugger.JsonLogging,
                configuration.Debugger.MinimumLogLevel.ToLogEventLevel());

            LoggingLog.AddSerilogDebuggerLogging(logger);
        }

        if (configuration.System != null)
        {
            if (OperatingSystem.IsWindows())
            {
                _ = serilogLoggerConfiguration.AddEventLogLogging(
                    configuration.System.WindowsEventLogSource,
                    configuration.System.WindowsEventLogName,
                    configuration.System.WindowsEventLogMachineName,
                    configuration.System.WindowsEventLogManageEventSource,
                    configuration.System.MinimumLogLevel.ToLogEventLevel());

                LoggingLog.AddSerilogWindowsEventLogLogging(logger);
            }
            else
            {
                _ = serilogLoggerConfiguration.AddSyslogLogging(
                    configuration.System.SyslogHost,
                    configuration.System.SyslogPort,
                    configuration.System.SyslogAppName,
                    configuration.System.MinimumLogLevel.ToLogEventLevel());

                LoggingLog.AddSerilogSyslogLogging(logger);
            }
        }

        if (configuration.OpenTelemetry != null)
        {
            _ = serilogLoggerConfiguration.AddOpenTelemetryLogging(
                configuration.OpenTelemetry.Uri!.ToString(),
                configuration.OpenTelemetry.Protocol ?? OtlpProtocol.HttpProtobuf,
                configuration.OpenTelemetry.Authorization,
                configuration.OpenTelemetry.MinimumLogLevel.ToLogEventLevel());

            LoggingLog.AddSerilogOpenTelemetryLogging(logger);
        }

        if (configuration.Seq != null)
        {
            _ = serilogLoggerConfiguration.AddSeqLogging(
                configuration.Seq.Uri!,
                configuration.Seq.ApiKey,
                configuration.Seq.MinimumLogLevel.ToLogEventLevel());

            LoggingLog.AddSerilogSeqLogging(logger);
        }

        builder?.Invoke(serilogLoggerConfiguration);

        _ = services
            .AddSerilogLogProvider(serilogLoggerConfiguration,
            logger);

        return services;
    }
}
