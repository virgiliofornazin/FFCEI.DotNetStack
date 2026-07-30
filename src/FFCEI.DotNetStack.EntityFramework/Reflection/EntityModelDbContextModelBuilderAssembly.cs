using FFCEI.DotNetStack.EntityFramework.Interfaces;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FFCEI.DotNetStack.EntityFramework.Reflection;

public sealed class EntityModelDbContextModelBuilderAssembly
{
    public OrderedDictionary<string, string?> EntityModelNamespaceDatabaseSchemaMap { get; private set; } = [];
    public HashSet<Type> EntityTypeConfigurationImplementations { get; private set; } = [];
    public HashSet<Type> EntityTypeModelImplementations { get; private set; } = [];
    public Assembly EntityModelAssembly { get; private set; }

    public EntityModelDbContextModelBuilderAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));

        EntityModelAssembly = assembly;

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
            return;
        }

        var entityModelNamespaceDatabaseSchemaTypes = assemblyTypes.Where(assemblyType => (assemblyType.Namespace != null) &&
            assemblyType.IsClass && assemblyType.Implements<IEntityModelNamespaceDatabaseSchema>()).ToHashSet();

        foreach (var entityModelNamespaceDatabaseSchemaType in entityModelNamespaceDatabaseSchemaTypes)
        {
            var databaseSchema = Activator.CreateInstance(entityModelNamespaceDatabaseSchemaType) as IEntityModelNamespaceDatabaseSchema;

            if (!string.IsNullOrWhiteSpace(entityModelNamespaceDatabaseSchemaType?.Namespace))
            {
                EntityModelNamespaceDatabaseSchemaMap[entityModelNamespaceDatabaseSchemaType.Namespace] = databaseSchema?.Name;
            }
        }

        EntityTypeConfigurationImplementations = [.. assemblyTypes.Where(assemblyType => (assemblyType.Namespace != null) &&
            assemblyType.GetInterfaces().Any(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))];

        EntityTypeModelImplementations = [.. assemblyTypes.Where(assemblyType => (assemblyType.Namespace != null) && assemblyType.IsEntityModelImplementationType())];
    }
}
