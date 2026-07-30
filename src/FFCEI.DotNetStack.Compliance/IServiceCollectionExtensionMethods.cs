using FFCEI.DotNetStack.Compliance.Common.Attributes;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;

namespace FFCEI.DotNetStack.Compliance;

public static class IServiceCollectionExtensionMethods
{
    private static readonly MethodInfo _methodIRedactionBuilderSetRedactor =
        typeof(IRedactionBuilder).GetMethods().Single(m => m.Name == nameof(IRedactionBuilder.SetRedactor) && m.IsGenericMethod);

    public static IServiceCollection AddComplianceRedaction(this IServiceCollection services,
        Action<IRedactionBuilder>? builder = null,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;

        _ = services.AddRedaction(redactionBuilder =>
        {
            var derivedTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(RedactorAttribute)));

            foreach (var type in derivedTypes)
            {
                var attribute = Activator.CreateInstance(type);

                if (attribute is RedactorAttribute redactorAttribute)
                {
                    var redactorType = redactorAttribute.RedactorType;
                    var classification = redactorAttribute.Classification;

                    var method = _methodIRedactionBuilderSetRedactor.MakeGenericMethod(redactorType);
                    var parameters = (DataClassificationSet[])[new DataClassificationSet(classification)];

                    _ = method.Invoke(redactionBuilder, [parameters]);
                }
            }

            _ = redactionBuilder.SetFallbackRedactor<ErasingRedactor>();

            builder?.Invoke(redactionBuilder);
        });

        ComplianceLog.ComplianceConfigured(logger);

        return services;
    }
}
