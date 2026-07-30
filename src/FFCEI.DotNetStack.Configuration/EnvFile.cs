using DotNetEnv;
using FFCEI.DotNetStack.Configuration.Configurations;
using FFCEI.DotNetStack.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32;
using System.Reflection;

namespace FFCEI.DotNetStack.Configuration;

public static class EnvFile
{
    private const string RegistryEnvFileLocationKey = ".envFileLocation";

    internal static IReadOnlyCollection<string>? ReadEnvFileLocationFromRegistryKey(RegistryKey registryRootKey, string subKey)
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        using var registryKey = registryRootKey.OpenSubKey(subKey);

        if (registryKey == null)
        {
            return null;
        }

        return ExceptionProtector.ExecuteFunctionAndReturnOrNullReference<IReadOnlyCollection<string>>(() =>
        {
            if (!OperatingSystem.IsWindows())
            {
                return null;
            }

            var rawValue = registryKey.GetValue(RegistryEnvFileLocationKey);

            if (rawValue == null)
            {
                return null;
            }

            var valueKind = registryKey.GetValueKind(RegistryEnvFileLocationKey);

            switch (valueKind)
            {
            case RegistryValueKind.String:
                {
                    return [rawValue.ToString()!];
                }
            case RegistryValueKind.ExpandString:
                {
                    return [rawValue.ToString()!];
                }
            case RegistryValueKind.MultiString:
                {
                    if (rawValue is IEnumerable<string> values)
                    {
                        return values.ToList().AsReadOnly();
                    }

                    return [rawValue.ToString()!];
                }
            default:
                {
                    return null;
                }
            }
        });
    }

    public static void MergeVariablesFromEnvFile(EnvFileLoadOptions options, Dictionary<string, string> result, IReadOnlyCollection<KeyValuePair<string, string>> values, Action<string>? valueDefined = null)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(result, nameof(result));
        ArgumentNullException.ThrowIfNull(values, nameof(values));

        foreach (var value in values)
        {
            if (result.ContainsKey(value.Key))
            {
                if (options.IgnoreAlreadyDefinedVariables)
                {
                    continue;
                }

                if (!options.AllowOverridePreviousVariables)
                {
                    throw new InvalidOperationException($"Value '{value.Key}' already defined in this (or other) file in this location");
                }
            }

            result[value.Key] = value.Value;

            ExceptionProtector.ExecuteAction(() => valueDefined?.Invoke(value.Key));
        }
    }

    private static LoadOptions FromEnvFileLoadOptions(EnvFileLoadOptions options)
    {
        return new LoadOptions(setEnvVars: options.SetEnvironmentVariables,
            clobberExistingVars: !options.IgnoreAlreadyDefinedVariables && options.AllowOverridePreviousVariables, onlyExactPath: true);
    }

    public static IReadOnlyCollection<KeyValuePair<string, string>> LoadSpecificEnvFile(string envFile, EnvFileLoadOptions? options = null, ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;

        ConfigurationLog.AttemptingToLoadEnvironmentVariablesFile(logger, envFile);

        options ??= new()
        {
            SetEnvironmentVariables = true,
            IgnoreAlreadyDefinedVariables = false,
            AllowOverridePreviousVariables = true
        };

        var result = Env.Load(envFile, options: FromEnvFileLoadOptions(options)).ToList().AsReadOnly();

        if (result.Count == 0)
        {
            ConfigurationLog.EmptyEnvironmentVariablesFile(logger, envFile);
        }
        else
        {
            ConfigurationLog.LoadedEnvironmentVariablesFile(logger, result.Count, envFile);
        }

        return result;
    }

    public static IReadOnlyCollection<KeyValuePair<string, string>> LoadDefaultEnvFile(EnvFileLoadOptions? options = null, ILogger? logger = null)
        => LoadSpecificEnvFile(".env", options, logger);

    public static IReadOnlyCollection<KeyValuePair<string, string>> LoadDefaultLocalEnvFile(EnvFileLoadOptions? options = null, ILogger? logger = null)
        => LoadSpecificEnvFile(".env.local", options, logger);

    public static IReadOnlyCollection<KeyValuePair<string, string>> LoadAllEnvFilesFromLocation(string envFileLocation, EnvFileLoadOptions? options = null, ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;

        options ??= new()
        {
            SetEnvironmentVariables = false,
            IgnoreAlreadyDefinedVariables = false,
            AllowOverridePreviousVariables = true
        };

        var result = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(envFileLocation) && Directory.Exists(envFileLocation))
        {
            var names = Directory.GetFiles(envFileLocation, "*", new EnumerationOptions()
            {
                RecurseSubdirectories = false,
                ReturnSpecialDirectories = false,
                AttributesToSkip = FileAttributes.Directory
            });

            if (names != null && names.Length > 0)
            {
                names.Sort();

                foreach (var name in names)
                {
                    var fullPath = Path.Combine(envFileLocation, name);
                    var values = LoadSpecificEnvFile(fullPath, options, logger);

                    MergeVariablesFromEnvFile(options, result, values);
                }
            }
        }

        return result.ToList().AsReadOnly();
    }

    public static IReadOnlyCollection<KeyValuePair<string, string>> LoadDefaultEnvFilesFromLocation(string envFileLocation, EnvFileLoadOptions? options = null, ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;

        const string envFileName = ".env";

        ConfigurationLog.SearchingEnvironmentVariablesFileIntoLocation(logger, envFileName, envFileLocation);

        if (!string.IsNullOrWhiteSpace(envFileLocation) && Directory.Exists(envFileLocation))
        {
            var fullPath = Path.Combine(envFileLocation, envFileName);

            if (File.Exists(fullPath))
            {
                return LoadSpecificEnvFile(fullPath, options, logger);
            }
        }

        return [];
    }

    public static IReadOnlyCollection<KeyValuePair<string, string>> LoadDefaultLocalEnvFilesFromLocation(string envFileLocation, EnvFileLoadOptions? options = null, ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;

        const string envFileName = ".env.local";

        ConfigurationLog.SearchingEnvironmentVariablesFileIntoLocation(logger, envFileName, envFileLocation);

        if (!string.IsNullOrWhiteSpace(envFileLocation) && Directory.Exists(envFileLocation))
        {
            var fullPath = Path.Combine(envFileLocation, envFileName);

            if (File.Exists(fullPath))
            {
                return LoadSpecificEnvFile(fullPath, options, logger);
            }
        }

        return [];
    }

    public static IReadOnlyCollection<KeyValuePair<string, string>> LoadEnvFiles(EnvFileLoadOptions? options = null, ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;

        options ??= new()
        {
            SetEnvironmentVariables = false,
            IgnoreAlreadyDefinedVariables = false,
            AllowOverridePreviousVariables = true
        };

        var entryAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
        var assemblyName = entryAssembly?.GetName().Name;
        var result = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(assemblyName))
        {
            if (OperatingSystem.IsWindows())
            {
#pragma warning disable CA1416 // Validate platform compatibility
                var envFileLocations = ReadEnvFileLocationFromRegistryKey(Registry.LocalMachine, $"SOFTWARE\\FFCEI.DotNetStack.Configuration\\{assemblyName}");

                if (envFileLocations != null && (envFileLocations.Count > 0))
                {
                    foreach (var envFileLocation in envFileLocations)
                    {
                        var values = LoadDefaultEnvFilesFromLocation(envFileLocation, options, logger);

                        MergeVariablesFromEnvFile(options, result, values);
                    }

                    foreach (var envFileLocation in envFileLocations)
                    {
                        var values = LoadDefaultLocalEnvFilesFromLocation(envFileLocation, options, logger);

                        MergeVariablesFromEnvFile(options, result, values);
                    }
                }

                envFileLocations = ReadEnvFileLocationFromRegistryKey(Registry.CurrentUser, $"SOFTWARE\\FFCEI.DotNetStack.Configuration\\{assemblyName}");

                if (envFileLocations != null && (envFileLocations.Count > 0))
                {
                    foreach (var envFileLocation in envFileLocations)
                    {
                        var values = LoadDefaultEnvFilesFromLocation(envFileLocation, options, logger);

                        MergeVariablesFromEnvFile(options, result, values);
                    }

                    foreach (var envFileLocation in envFileLocations)
                    {
                        var values = LoadDefaultLocalEnvFilesFromLocation(envFileLocation, options, logger);

                        MergeVariablesFromEnvFile(options, result, values);
                    }
                }

                var defaultValues = LoadDefaultEnvFile(options, logger);

                MergeVariablesFromEnvFile(options, result, defaultValues);

                var defaultLocalValues = LoadDefaultLocalEnvFile(options, logger);

                MergeVariablesFromEnvFile(options, result, defaultLocalValues);
#pragma warning restore CA1416 // Validate platform compatibility
            }
            else
            {
                var envFileLocation = Path.Combine("/etc", assemblyName);

                var locationValues = LoadDefaultEnvFilesFromLocation(envFileLocation, options, logger);

                MergeVariablesFromEnvFile(options, result, locationValues);

                var locationLocalValues = LoadDefaultLocalEnvFilesFromLocation(envFileLocation, options, logger);

                MergeVariablesFromEnvFile(options, result, locationLocalValues);

                var defaultValues = LoadDefaultEnvFile(options, logger);

                MergeVariablesFromEnvFile(options, result, defaultValues);

                var defaultLocalValues = LoadDefaultLocalEnvFile(options, logger);

                MergeVariablesFromEnvFile(options, result, defaultLocalValues);
            }
        }
        else
        {
            var defaultValues = LoadDefaultEnvFile(options, logger);

            MergeVariablesFromEnvFile(options, result, defaultValues);

            var defaultLocalValues = LoadDefaultLocalEnvFile(options, logger);

            MergeVariablesFromEnvFile(options, result, defaultLocalValues);
        }

        return result.ToList().AsReadOnly();
    }
}
