using FFCEI.DotNetStack.Models.Interfaces;
using FFCEI.DotNetStack.Models.Reflection;

namespace FFCEI.DotNetStack.Models;

public static class IEntityModelExtensionMethods
{
    public static bool IsLazyLoadingProxy(this IEntityModel entityModel)
    {
        ArgumentNullException.ThrowIfNull(entityModel, nameof(entityModel));

        return entityModel.GetType().IsLazyLoadingProxyType();
    }

    public static Type GetLazyLoadingProxyUnderlyingType(this IEntityModel entityModel)
    {
        ArgumentNullException.ThrowIfNull(entityModel, nameof(entityModel));

        return entityModel.GetType().GetLazyLoadingProxyUnderlyingType();
    }

    public static void CopyEntityModelIdsFrom(this IEntityModel destination, object source,
        Action<IEntityModel, object, string, object?>? copyCallback = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(destination, nameof(destination));

        var destinationType = destination.GetType();
        var sourceType = source.GetType();

        if (!destinationType.IsAssignableFrom(sourceType) && !sourceType.IsAssignableFrom(destinationType))
        {
            throw new InvalidOperationException("Source and destination must be assignable types.");
        }

        if (source is IDatabaseIdEntityModel sourceId && destination is IDatabaseIdEntityModel destinationId)
        {
            destinationId.Id = sourceId.Id;

            copyCallback?.Invoke(destination, source, nameof(IDatabaseIdEntityModel.Id), destinationId.Id);
        }

        if (source is IUniversalUniqueIdEntityModel sourceExternalId && destination is IUniversalUniqueIdEntityModel destinationExternalId)
        {
            destinationExternalId.UniversalUniqueId = sourceExternalId.UniversalUniqueId;

            copyCallback?.Invoke(destination, source, nameof(IUniversalUniqueIdEntityModel.UniversalUniqueId), destinationExternalId.UniversalUniqueId);
        }
    }

    public static void CopyEntityModelCreatedTimestampFrom(this IEntityModel destination, object source,
        Action<IEntityModel, object, string, object?>? copyCallback = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(destination, nameof(destination));

        var destinationType = destination.GetType();
        var sourceType = source.GetType();

        if (!destinationType.IsAssignableFrom(sourceType) && !sourceType.IsAssignableFrom(destinationType))
        {
            throw new InvalidOperationException("Source and destination must be assignable types.");
        }

        if (source is ITimestampTrackedEntityModel sourceTimestampTrackedModel && destination is ITimestampTrackedEntityModel destinationTimestampTrackedModel)
        {
            destinationTimestampTrackedModel.CreatedTimestamp = sourceTimestampTrackedModel.CreatedTimestamp;

            copyCallback?.Invoke(destination, source, nameof(ITimestampTrackedEntityModel.CreatedTimestamp), destinationTimestampTrackedModel.CreatedTimestamp);
        }
    }

    public static void CopyEntityModelPropertiesFrom(this IEntityModel destination, object source,
        bool copyEntityModelIds = false, bool copyLogicallyDeletedFlags = false, bool copyEntityModelTimestamps = false,
        Action<IEntityModel, object, string, object?>? copyCallback = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(destination, nameof(destination));

        var destinationType = destination.GetType();
        var sourceType = source.GetType();

        if (!destinationType.IsAssignableFrom(sourceType) && !sourceType.IsAssignableFrom(destinationType))
        {
            throw new InvalidOperationException("Source and destination must be assignable types.");
        }

        if (copyEntityModelIds)
        {
            destination.CopyEntityModelIdsFrom(source, copyCallback);
        }

        if (copyLogicallyDeletedFlags && source is ILogicallyDeletableEntityModel sourceLogicallyDeletableModel && destination is ILogicallyDeletableEntityModel destinationLogicallyDeletableModel)
        {
            destinationLogicallyDeletableModel.IsLogicallyDeleted = sourceLogicallyDeletableModel.IsLogicallyDeleted;

            copyCallback?.Invoke(destination, source, nameof(ILogicallyDeletableEntityModel.IsLogicallyDeleted), destinationLogicallyDeletableModel.IsLogicallyDeleted);

            destinationLogicallyDeletableModel.LogicallyDeletedTimestamp = sourceLogicallyDeletableModel.LogicallyDeletedTimestamp;

            copyCallback?.Invoke(destination, source, nameof(ILogicallyDeletableEntityModel.LogicallyDeletedTimestamp), destinationLogicallyDeletableModel.LogicallyDeletedTimestamp);

            destinationLogicallyDeletableModel.WasLogicallyDeletedCascaded = sourceLogicallyDeletableModel.WasLogicallyDeletedCascaded;

            copyCallback?.Invoke(destination, source, nameof(ILogicallyDeletableEntityModel.WasLogicallyDeletedCascaded), destinationLogicallyDeletableModel.WasLogicallyDeletedCascaded);
        }

        if (copyEntityModelTimestamps && source is ITimestampTrackedEntityModel sourceTimestampTrackedModel && destination is ITimestampTrackedEntityModel destinationTimestampTrackedModel)
        {
            destinationTimestampTrackedModel.CreatedTimestamp = sourceTimestampTrackedModel.CreatedTimestamp;

            copyCallback?.Invoke(destination, source, nameof(ITimestampTrackedEntityModel.CreatedTimestamp), destinationTimestampTrackedModel.CreatedTimestamp);

            destinationTimestampTrackedModel.UpdatedTimestamp = sourceTimestampTrackedModel.UpdatedTimestamp;

            copyCallback?.Invoke(destination, source, nameof(ITimestampTrackedEntityModel.UpdatedTimestamp), destinationTimestampTrackedModel.UpdatedTimestamp);
        }

        var sourceEntityModelProperyMap = EntityModelPropertyMapCache.Instance.GetOrAdd(sourceType);
        var destinatinEntityModelProperyMap = EntityModelPropertyMapCache.Instance.GetOrAdd(destinationType);

        foreach (var sourceProperty in sourceEntityModelProperyMap.Properties)
        {
            if (sourceProperty.PropertyInfo.PropertyType.IsEntityModelNavigationCollection())
            {
                continue;
            }

            var destinationProperty = destinatinEntityModelProperyMap.Properties.FirstOrDefault(property => property.Name == sourceProperty.Name);

            if (destinationProperty == null)
            {
                continue;
            }

            if (!destinationProperty.PropertyInfo.PropertyType.IsAssignableFrom(sourceProperty.PropertyInfo.PropertyType))
            {
                throw new InvalidOperationException($"Source and destination property types of {sourceProperty.Name} must be assignable types.");
            }

            var propertyValue = sourceProperty.PropertyInfo.GetValue(source);

            destinationProperty.PropertyInfo.SetValue(destination, propertyValue);

            copyCallback?.Invoke(destination, source, destinationProperty.Name, propertyValue);
        }
    }

    public static void GenerateModelAutomaticPropertiesForSave(this IEntityModel entityModel, DateTimeOffset timestamp)
    {
        if (entityModel is IUniversalUniqueIdEntityModel universalUniqueIdEntityModel)
        {
            _ = universalUniqueIdEntityModel.GenerateModelUniversalUniqueId();
        }

        if (entityModel is ITimestampTrackedEntityModel timestampTrackedModel)
        {
            timestampTrackedModel.TouchModelTimestamps(timestamp);
        }

        if (entityModel is ILogicallyDeletableEntityModel logicallyDeletableEntityModel)
        {
            if (logicallyDeletableEntityModel.IsLogicallyDeleted)
            {
                logicallyDeletableEntityModel.LogicallyDeletedTimestamp ??= timestamp;
            }
            else
            {
                logicallyDeletableEntityModel.LogicallyDeletedTimestamp = null;
                logicallyDeletableEntityModel.WasLogicallyDeletedCascaded = false;
            }
        }
    }
}
