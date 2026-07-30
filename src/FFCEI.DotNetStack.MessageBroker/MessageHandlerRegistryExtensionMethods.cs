using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Configurations;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using System.Reflection;

namespace FFCEI.DotNetStack.MessageBroker;

public static class MessageHandlerRegistryExtensionMethods
{
    private static IReadOnlyCollection<Type> ExtractHandlerTypesFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));

        Type[] assemblyTypes;

        try
        {
            assemblyTypes = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            assemblyTypes = [.. ex.Types.Where(t => t != null).Cast<Type>()];
        }

        if (assemblyTypes.Length == 0)
        {
            return assemblyTypes;
        }

        var handlerTypes = assemblyTypes.Where(type => type.Implements<IMessageEnvelopeHandler>() &&
            !type.IsAbstract && !type.IsInterface && !type.IsGenericTypeDefinition &&
            (type.GetConstructor(Type.EmptyTypes) != null)).ToList();

        return handlerTypes;
    }

    internal static Type? LoadHandler(MessageBrokerAutomaticReceiverHandlerConfiguration handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));
        ArgumentException.ThrowIfNullOrWhiteSpace(handler.HandlerTypename, nameof(handler.HandlerTypename));
        Type? handlerType;

        if (string.IsNullOrEmpty(handler.AssemblyFileName))
        {
            handlerType = TypeExtensionMethods.FindType(handler.HandlerTypename);
        }
        else
        {
            var assemblyFileName = Path.GetFileNameWithoutExtension(handler.AssemblyFileName);

            var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .FirstOrDefault(a => string.Equals(Path.GetFileNameWithoutExtension(a.Location), assemblyFileName, StringComparison.OrdinalIgnoreCase)) ??
                    Assembly.Load(handler.AssemblyFileName);

            handlerType = loadedAssembly?.GetType(handler.HandlerTypename);
        }

        return handlerType;
    }

    public static int LoadAssemblyHandlerTypes(this MessageHandlerRegistry registry, Assembly assembly, bool skipLoadingErrors = false)
    {
        ArgumentNullException.ThrowIfNull(registry, nameof(registry));

        var loaded = 0;
        var handlerTypes = ExtractHandlerTypesFromAssembly(assembly);

        foreach (var handlerType in handlerTypes)
        {
            IMessageEnvelopeHandler? handler;

            try
            {
                handler = Activator.CreateInstance(handlerType) as IMessageEnvelopeHandler;
            }
            catch (Exception exception) when (skipLoadingErrors && exception is not OutOfMemoryException)
            {
                continue;
            }

            if (handler == null)
            {
                if (skipLoadingErrors)
                {
                    continue;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot instantiate message handler '{handlerType.FullName}'");
                }
            }

            var messageType = handler.MessageType;
            var registered = registry.TryAddMessageHandler(handler);

            if (!registered)
            {
                if (skipLoadingErrors)
                {
                    continue;
                }
                else
                {
                    throw new InvalidOperationException($"Handler for type '{messageType}' already registered in this registry");
                }
            }

            ++loaded;
        }

        return loaded;
    }
}
