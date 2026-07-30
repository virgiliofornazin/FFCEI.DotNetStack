using FFCEI.DotNetStack.Models;
using FFCEI.DotNetStack.Models.Interfaces;
using FFCEI.DotNetStack.Models.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FFCEI.DotNetStack.EntityFramework;

public static class IEntityModelExtensionMethods
{
    public static EntityModelPropertyMap GetEntityModelPropertyMap(this IEntityModel entity, DbContext context)
    {
        var entityType = context.ClrTypeOfEntityType(entity) ??
            throw new InvalidOperationException($"Entity type '{entity.GetType().Name}' is not part of the DbContext model");

        return EntityModelPropertyMapCache.Instance.GetOrAdd(entityType);
    }

    public static EntityModelPropertyMap GetEntityModelPropertyMap(this IEntityModel entity)
    {
        var entityType = entity.GetLazyLoadingProxyUnderlyingType();

        return EntityModelPropertyMapCache.Instance.GetOrAdd(entityType);
    }

    public static void AddEntityToForeignKeyReferencesNavigations(this IEntityModel entity, DbContext context, EntityModelPropertyMap entityPropertyMap)
        => AddRemoveEntityInForeignKeyReferencesNavigationsOf(entity, context, entityPropertyMap, adding: true);
    public static void RemoveEntityFromForeignKeyReferencesNavigations(this IEntityModel entity, DbContext context, EntityModelPropertyMap entityPropertyMap)
        => AddRemoveEntityInForeignKeyReferencesNavigationsOf(entity, context, entityPropertyMap, adding: false);
    public static void AddRemoveEntityInForeignKeyReferencesNavigations(this IEntityModel entity, DbContext context, EntityModelPropertyMap entityPropertyMap, bool adding)
        => AddRemoveEntityInForeignKeyReferencesNavigationsOf(entity, context, entityPropertyMap, adding);

    private static readonly MethodInfo? _methodIEntityModelExtensionMethodsAddEntityFromCollection = typeof(IEntityModelExtensionMethods)
        .GetMethod(nameof(AddEntityFromCollection), BindingFlags.Static | BindingFlags.NonPublic);

    private static readonly MethodInfo? _methodIEntityModelExtensionMethodsRemoveEntityFromCollection = typeof(IEntityModelExtensionMethods)
        .GetMethod(nameof(RemoveEntityFromCollection), BindingFlags.Static | BindingFlags.NonPublic);

    internal static void AddRemoveEntityInForeignKeyReferencesNavigationsOf(object? entity, DbContext context, EntityModelPropertyMap entityPropertyMap, bool adding)
    {
        if (entity == null)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(entityPropertyMap, nameof(entityPropertyMap));

        if (_methodIEntityModelExtensionMethodsAddEntityFromCollection == null)
        {
            throw new InvalidOperationException($"Methods '{nameof(AddEntityFromCollection)}' not found by reflection; incompatible library version");
        }

        if (_methodIEntityModelExtensionMethodsRemoveEntityFromCollection == null)
        {
            throw new InvalidOperationException($"Methods '{nameof(RemoveEntityFromCollection)}' not found by reflection; incompatible library version");
        }

        var entityType = entity.GetType().GetLazyLoadingProxyUnderlyingType();

        var addEntityFromCollectionMethod = _methodIEntityModelExtensionMethodsAddEntityFromCollection
            .MakeGenericMethod(entityType);

        var removeEntityFromCollectionMethod = _methodIEntityModelExtensionMethodsRemoveEntityFromCollection
            .MakeGenericMethod(entityType);

        foreach (var foreignKeyReference in entityPropertyMap.ForeignKeyReferences)
        {
            if (string.IsNullOrWhiteSpace(foreignKeyReference.ReverseNavigationPropertyName))
            {
                continue;
            }

            var foreignKeyEntityObject = foreignKeyReference.EntityProperty.GetValue(entity);

            if (foreignKeyEntityObject is not IEntityModel foreignKeyEntity)
            {
                continue;
            }

            var foreignEntityPropertyMap = foreignKeyEntity.GetEntityModelPropertyMap(context);

            var propertyMetadata = foreignEntityPropertyMap.Properties.FirstOrDefault(
                property => property.Name == foreignKeyReference.ReverseNavigationPropertyName) ??
                throw new InvalidOperationException($"Navigation property '{foreignKeyReference.ReverseNavigationPropertyName}' not found on entity '{foreignKeyEntity.GetType().Name}'");

            var navigation = propertyMetadata.PropertyInfo.GetValue(foreignKeyEntity);

            if (navigation == null)
            {
                continue;
            }

            var isWriteableNavigation = navigation.GetType().IsWriteableEntityModelNavigationCollection();

            if (isWriteableNavigation)
            {
                _ = (adding ? addEntityFromCollectionMethod : removeEntityFromCollectionMethod).Invoke(null, [navigation, entity]);
            }
        }
    }

    private static void AddEntityFromCollection<TEntityModel>(IEnumerable<TEntityModel> entities, TEntityModel entity) where TEntityModel : IEntityModel
    {
        if (entities is ICollection<TEntityModel> writableEntities)
        {
            if (!writableEntities.Contains(entity))
            {
                writableEntities.Add(entity);
            }
        }
    }

    private static void RemoveEntityFromCollection<TEntityModel>(IEnumerable<TEntityModel> entities, TEntityModel entity) where TEntityModel : IEntityModel
    {
        if (entities is ICollection<TEntityModel> writableEntities)
        {
            _ = writableEntities.Remove(entity);
        }
    }
}
