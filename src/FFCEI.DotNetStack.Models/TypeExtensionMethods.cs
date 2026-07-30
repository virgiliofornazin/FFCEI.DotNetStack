using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Models.Interfaces;
using System.Reflection;

namespace FFCEI.DotNetStack.Models;

public static class TypeExtensionMethods
{
    public static readonly Type[] AllEntityModelInterfaceTypes =
    [
        typeof(IEntityModel),
        typeof(IUniqueIdEntityModel),
        typeof(IExternalUniqueIdEntityModel),
        typeof(ILogicallyDeletableEntityModel),
        typeof(ITimestampTrackedEntityModel)
    ];

    public static readonly Type[] AllEntityModelImplementationTypes =
    [
        typeof(EntityModel),
        typeof(UniqueIdEntityModel),
        typeof(UniqueIdLogicallyDeletableEntityModel),
        typeof(UniqueIdLogicallyDeletableTimestampTrackedEntityModel),
        typeof(UniqueIdTimestampTrackedEntityModel),
        typeof(UniqueIdExternalUniqueIdEntityModel),
        typeof(UniqueIdExternalUniqueIdLogicallyDeletableEntityModel),
        typeof(UniqueIdExternalUniqueIdLogicallyDeletableTimestampTrackedEntityModel),
        typeof(UniqueIdExternalUniqueIdTimestampTrackedEntityModel),
        typeof(ExternalUniqueIdEntityModel),
        typeof(ExternalUniqueIdLogicallyDeletableEntityModel),
        typeof(ExternalUniqueIdLogicallyDeletableTimestampTrackedEntityModel),
        typeof(ExternalUniqueIdTimestampTrackedEntityModel),
        typeof(LogicallyDeletableEntityModel),
        typeof(LogicallyDeletableTimestampTrackedEntityModel),
        typeof(TimestampTrackedEntityModel)
    ];

    public static bool IsLazyLoadingProxyType(this Type entityModelType)
    {
        ArgumentNullException.ThrowIfNull(entityModelType, nameof(entityModelType));

        return entityModelType.Namespace == "Castle.Proxies";
    }

    public static Type GetLazyLoadingProxyUnderlyingType(this Type entityModelType)
        => (IsLazyLoadingProxyType(entityModelType) ? entityModelType.BaseType : null) ?? entityModelType;

    public static Type[] ImplementedEntityModelTypes(this Type entityModelType)
    {
        ArgumentNullException.ThrowIfNull(entityModelType, nameof(entityModelType));

        var derivedEntityModelInterfaces = entityModelType.GetInterfaces()
            .Where(entityInterface => entityInterface.Implements<IEntityModel>() && AllEntityModelInterfaceTypes.Contains(entityInterface));

        var implementedEntityModelImplementations = entityModelType.InheritanceHierarchy()
            .Where(entityType => entityType.Implements<IEntityModel>() && AllEntityModelImplementationTypes.Contains(entityType));

        return [.. derivedEntityModelInterfaces, .. implementedEntityModelImplementations];
    }

    public static Type[] ImplementedEntityModelTypes<TEntityModel>(this TEntityModel entityModel) where TEntityModel : class
    {
        ArgumentNullException.ThrowIfNull(entityModel, nameof(entityModel));

        return ImplementedEntityModelTypes(entityModel.GetType());
    }

    public static bool IsEntityModelDerivedType(this Type entityModelType)
        => entityModelType.ImplementedEntityModelTypes().Length != 0;

    public static bool IsEntityModelImplementationType(this Type entityModelType)
    {
        ArgumentNullException.ThrowIfNull(entityModelType, nameof(entityModelType));

        if (entityModelType.IsInterface || entityModelType.IsAbstract || !entityModelType.IsClass)
        {
            return false;
        }

        return entityModelType.IsEntityModelDerivedType();
    }

    public static Type[] EntityModelImplementationTypes(this Type entityModelType)
    {
        ArgumentNullException.ThrowIfNull(entityModelType, nameof(entityModelType));

        if (entityModelType.IsInterface || entityModelType.IsAbstract || !entityModelType.IsClass)
        {
            return [];
        }

        var implementedEntityModelTypes = entityModelType.ImplementedEntityModelTypes();

        if (implementedEntityModelTypes.Length == 0)
        {
            return [];
        }

        var derivedEntityModelInterfaces = entityModelType.GetInterfaces()
            .Where(entityInterface => entityInterface.Implements<IEntityModel>()).Except(implementedEntityModelTypes);

        var implementedEntityModelImplementations = entityModelType.InheritanceHierarchy()
            .Where(entityType => entityType.Implements<IEntityModel>()).Except(implementedEntityModelTypes);

        return [.. derivedEntityModelInterfaces, .. implementedEntityModelImplementations];
    }

    public static bool IsEntityModelNavigation(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        return type.Implements<IEntityModel>();
    }

    public static bool IsEntityModelNavigationCollection(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        var interfaces = type.GetInterfaces();

        return interfaces.Any(type => type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>) &&
            type.GetGenericArguments().Length != 0 &&
            type.GetGenericArguments()[0].Implements<IEntityModel>());
    }

    public static bool IsWriteableEntityModelNavigationCollection(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        var interfaces = type.GetInterfaces();

        return interfaces.Any(type => type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(ICollection<>) &&
            type.GetGenericArguments().Length != 0 &&
            type.GetGenericArguments()[0].Implements<IEntityModel>());
    }

    public static TAttribute? GetCustomAttributeFromEntityModelImplementationsOrInterface<TAttribute>(this Type type) where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        var attribute = type.GetCustomAttribute<TAttribute>(inherit: true);

        if (attribute != null)
        {
            return attribute;
        }

        var entityModelInterfaces = type.GetInterfaceHierarchy().Where(interfaceType => interfaceType.IsEntityModelDerivedType());

        foreach (var entityModelInterface in entityModelInterfaces)
        {
            attribute = entityModelInterface.GetCustomAttribute<TAttribute>(inherit: false);

            if (attribute != null)
            {
                break;
            }
        }

        return attribute;
    }

    public static IReadOnlyCollection<TAttribute> GetCustomAttributesFromEntityModelImplementationsOrInterface<TAttribute>(this Type type) where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        var attributes = type.GetCustomAttributes<TAttribute>(inherit: true).ToList();

        var entityModelInterfaces = type.GetInterfaces().Where(interfaceType => interfaceType.IsEntityModelDerivedType());

        foreach (var entityModelInterface in entityModelInterfaces)
        {
            var interfaceAttributes = entityModelInterface.GetCustomAttributes<TAttribute>(inherit: false);

            attributes.AddRange(interfaceAttributes);
        }

        return attributes.Distinct().ToList();
    }
}
