using FFCEI.DotNetStack.Models;
using FFCEI.DotNetStack.Models.Interfaces;
using FFCEI.DotNetStack.Models.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FFCEI.DotNetStack.EntityFramework;

public static class EntityEntryExtensionMethods
{
    public static void AddEntityToForeignKeyReferencesNavigations(this EntityEntry entityEntry, DbContext context, EntityModelPropertyMap entityPropertyMap)
    {
        ArgumentNullException.ThrowIfNull(entityEntry, nameof(entityEntry));

        IEntityModelExtensionMethods.AddRemoveEntityInForeignKeyReferencesNavigationsOf(entityEntry.Entity, context, entityPropertyMap, adding: true);
    }

    public static void RemoveEntityFromForeignKeyReferencesNavigations(this EntityEntry entityEntry, DbContext context, EntityModelPropertyMap entityPropertyMap)
    {
        ArgumentNullException.ThrowIfNull(entityEntry, nameof(entityEntry));

        IEntityModelExtensionMethods.AddRemoveEntityInForeignKeyReferencesNavigationsOf(entityEntry.Entity, context, entityPropertyMap, adding: false);
    }

    public static void AddRemoveEntityInForeignKeyReferencesNavigations(this EntityEntry entityEntry, DbContext context, EntityModelPropertyMap entityPropertyMap, bool adding)
    {
        ArgumentNullException.ThrowIfNull(entityEntry, nameof(entityEntry));

        IEntityModelExtensionMethods.AddRemoveEntityInForeignKeyReferencesNavigationsOf(entityEntry.Entity, context, entityPropertyMap, adding);
    }

    public static void UpdateChangeTrackerIdPropertiessFrom<TEntityModel>(this EntityEntry entityEntry, TEntityModel entityModel) where TEntityModel : class, IEntityModel
    {
        ArgumentNullException.ThrowIfNull(entityEntry, nameof(entityEntry));
        ArgumentNullException.ThrowIfNull(entityModel, nameof(entityModel));

        if (entityModel is IDatabaseIdEntityModel databaseIdEntityModel)
        {
            entityEntry.Property(nameof(IDatabaseIdEntityModel.Id)).CurrentValue = databaseIdEntityModel.Id;
        }

        if (entityModel is IUniversalUniqueIdEntityModel universalUniqueIdEntityModel)
        {
            entityEntry.Property(nameof(IUniversalUniqueIdEntityModel.UniversalUniqueId)).CurrentValue = universalUniqueIdEntityModel.UniversalUniqueId;
        }

        entityEntry.DetectChanges();
    }

    private static void HandlePropertyCopy(this IEntityModel destination, EntityEntry destinationEntry, IEntityModel source, string propertyName, object? propertyValue,
        Action<IEntityModel, EntityEntry, IEntityModel, string, object?>? copyCallback = null)
    {
        var property = destinationEntry.Properties.FirstOrDefault(property => property.Metadata.Name == propertyName)
            ?? throw new InvalidOperationException($"Property '{propertyName}' was not found among the model {destinationEntry.Entity.GetType().Name} tracked entry properties.");

        property.IsModified = true;
        property.CurrentValue = propertyValue;

        copyCallback?.Invoke(destination, destinationEntry, source, propertyName, propertyValue);
    }

    public static void CopyEntityModelAndEntryIdsFrom(this EntityEntry destinationEntry, IEntityModel destination, IEntityModel source,
        Action<IEntityModel, EntityEntry, IEntityModel, string, object?>? copyCallback = null)
    {
        ArgumentNullException.ThrowIfNull(destinationEntry, nameof(destinationEntry));

        destination.CopyEntityModelIdsFrom(source, (_, _, propertyName, propertyValue) =>
        {
            HandlePropertyCopy(destination, destinationEntry, source, propertyName, propertyValue, copyCallback);
        });

        destinationEntry.DetectChanges();
    }

    public static void CopyEntityModelAndEntryIdsAndCreatedTimestampFrom(this EntityEntry destinationEntry, IEntityModel destination, IEntityModel source,
        Action<IEntityModel, EntityEntry, IEntityModel, string, object?>? copyCallback = null)
    {
        ArgumentNullException.ThrowIfNull(destinationEntry, nameof(destinationEntry));

        destination.CopyEntityModelIdsFrom(source, (_, _, propertyName, propertyValue) =>
        {
            HandlePropertyCopy(destination, destinationEntry, source, propertyName, propertyValue, copyCallback);
        });

        destination.CopyEntityModelCreatedTimestampFrom(source, (_, _, propertyName, propertyValue) =>
        {
            HandlePropertyCopy(destination, destinationEntry, source, propertyName, propertyValue, copyCallback);
        });

        destinationEntry.DetectChanges();
    }

    public static void CopyEntityModelAndEntryPropertiesFrom(this EntityEntry destinationEntry, IEntityModel destination, IEntityModel source,
        bool copyEntityModelIds = false, bool copyLogicallyDeletedFlags = false, bool copyEntityModelTimestamps = false,
        Action<IEntityModel, EntityEntry, IEntityModel, string, object?>? copyCallback = null)
    {
        ArgumentNullException.ThrowIfNull(destinationEntry, nameof(destinationEntry));

        destination.CopyEntityModelPropertiesFrom(source, copyEntityModelIds, copyLogicallyDeletedFlags, copyEntityModelTimestamps, (_, _, propertyName, propertyValue) =>
        {
            HandlePropertyCopy(destination, destinationEntry, source, propertyName, propertyValue, copyCallback);
        });

        destinationEntry.DetectChanges();
    }
}
