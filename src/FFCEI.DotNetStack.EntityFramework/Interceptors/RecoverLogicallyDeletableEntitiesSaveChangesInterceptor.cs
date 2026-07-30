using FFCEI.DotNetStack.EntityFramework.Metadata.Conventions;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Models;
using FFCEI.DotNetStack.Models.Interfaces;
using FFCEI.DotNetStack.Models.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.EntityFramework.Interceptors;

public sealed class RecoverLogicallyDeletableEntitiesSaveChangesInterceptor
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));

        RecoverLogicallyDeletableEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));

        await RecoverLogicallyDeletableEntitiesAsync(eventData.Context, true, cancellationToken).ConfigureAwait(false);

        return await base.SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    private static void RecoverLogicallyDeletableEntities(DbContext? context)
    {
        EnsureCompletedSynchronously(RecoverLogicallyDeletableEntitiesAsync(context, isAsync: false, CancellationToken.None));
    }

    private static void EnsureCompletedSynchronously(ValueTask task)
    {
        if (!task.IsCompleted)
        {
            throw new InvalidOperationException(
                "Expected synchronous completion on the non-async logical-delete recovery path, but the operation did not complete synchronously.");
        }

        task.GetAwaiter().GetResult();
    }

    private static ValueTask RecoverLogicallyDeletableEntitiesAsync(DbContext? context, bool isAsync, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var entries = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList();

        if (entries.Count == 0)
        {
            return ValueTask.CompletedTask;
        }

        var logger = context.GetService<ILoggerFactory>().CreateLogger<RecoverLogicallyDeletableEntitiesSaveChangesInterceptor>();
        var modifiedEntries = new Stack<EntityEntry>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);

        return isAsync
            ? new ValueTask(DoRecoverLogicallyDeletableEntitiesAsync(context, isAsync, entries, modifiedEntries, visited, logger, cancellationToken))
            : DoRecoverLogicallyDeletableEntities(context, isAsync, entries, modifiedEntries, visited, logger, cancellationToken);
    }

    private static async Task DoRecoverLogicallyDeletableEntitiesLoopAsync(DbContext context, bool isAsync, List<EntityEntry> entries, Stack<EntityEntry> modifiedEntries, HashSet<object> visited, ILogger logger, CancellationToken cancellationToken)
    {
        foreach (var entry in entries)
        {
            if (entry.Entity is ILogicallyDeletableEntityModel)
            {
                await RecoverLogicallyDeletableEntityInternalAsync(context, entry, modifiedEntries, visited, isAsync, logger, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static async Task DoRecoverLogicallyDeletableEntitiesAsync(DbContext context, bool isAsync, List<EntityEntry> entries, Stack<EntityEntry> modifiedEntries, HashSet<object> visited, ILogger logger, CancellationToken cancellationToken)
    {
        var lazyLoadingEnabled = context.ChangeTracker.LazyLoadingEnabled;

        try
        {
            context.ChangeTracker.LazyLoadingEnabled = false;

            await DoRecoverLogicallyDeletableEntitiesLoopAsync(context, isAsync, entries, modifiedEntries, visited, logger, cancellationToken).ConfigureAwait(false);

            foreach (var modifiedEntry in modifiedEntries)
            {
                modifiedEntry.DetectChanges();
            }
        }
        finally
        {
            context.ChangeTracker.LazyLoadingEnabled = lazyLoadingEnabled;
        }
    }

    private static ValueTask DoRecoverLogicallyDeletableEntitiesLoop(DbContext context, bool isAsync, List<EntityEntry> entries, Stack<EntityEntry> modifiedEntries, HashSet<object> visited, ILogger logger, CancellationToken cancellationToken)
    {
        foreach (var entry in entries)
        {
            if (entry.Entity is ILogicallyDeletableEntityModel)
            {
                EnsureCompletedSynchronously(RecoverLogicallyDeletableEntityInternalAsync(context, entry, modifiedEntries, visited, isAsync, logger, cancellationToken));
            }
        }

        return ValueTask.CompletedTask;
    }

    private static ValueTask DoRecoverLogicallyDeletableEntities(DbContext context, bool isAsync, List<EntityEntry> entries, Stack<EntityEntry> modifiedEntries, HashSet<object> visited, ILogger logger, CancellationToken cancellationToken)
    {
        var lazyLoadingEnabled = context.ChangeTracker.LazyLoadingEnabled;

        try
        {
            context.ChangeTracker.LazyLoadingEnabled = false;

            EnsureCompletedSynchronously(DoRecoverLogicallyDeletableEntitiesLoop(context, isAsync, entries, modifiedEntries, visited, logger, cancellationToken));

            foreach (var modifiedEntry in modifiedEntries)
            {
                modifiedEntry.DetectChanges();
            }

            return ValueTask.CompletedTask;
        }
        finally
        {
            context.ChangeTracker.LazyLoadingEnabled = lazyLoadingEnabled;
        }
    }

    private static string[] IgnoreIsLogicallyDeletedQueryFilter = [QueryFilters.IsLogicallyDeletedQueryFilterName];

    private static ValueTask RecoverLogicallyDeletableEntityInternalAsync(
        DbContext context,
        EntityEntry entry,
        Stack<EntityEntry> modifiedEntries,
        HashSet<object> visited,
        bool isAsync,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (entry.Entity is not ILogicallyDeletableEntityModel entity)
        {
            return ValueTask.CompletedTask;
        }

        if (!visited.Add(entity))
        {
            return ValueTask.CompletedTask;
        }

        var entityModelPropertyMap = entity.GetEntityModelPropertyMap(context);

        if (entityModelPropertyMap.UniqueKeys.Count > 0)
        {
            var queryable = context.DbSetGeneric(entityModelPropertyMap.EntityModelType);

            ArgumentNullException.ThrowIfNull(queryable, nameof(queryable));

            if (isAsync)
            {
                return new ValueTask(ProcessUniqueKeyAndCascadeAsync(context, entry, entity, queryable, IgnoreIsLogicallyDeletedQueryFilter, entityModelPropertyMap, modifiedEntries, visited, isAsync, logger, cancellationToken));
            }
            else
            {
#pragma warning disable CA1849 // Call async methods when in an async method
                var existing = queryable.FindExistingByUniqueKeyGeneric(entity, IgnoreIsLogicallyDeletedQueryFilter, ignoreAutoIncludes: true, tracking: true);
#pragma warning restore CA1849 // Call async methods when in an async method

                ProcessExistingEntity(context, entry, entity, existing, modifiedEntries, logger);
            }
        }

        return ProcessCascadeReferences(context, entity, entityModelPropertyMap, modifiedEntries, visited, isAsync, logger, cancellationToken);
    }

    private static async Task ProcessUniqueKeyAndCascadeAsync(
        DbContext context,
        EntityEntry entry,
        ILogicallyDeletableEntityModel entity,
        IQueryable queryable,
        IReadOnlyCollection<string>? ignoreQueryFilters,
        EntityModelPropertyMap entityModelPropertyMap,
        Stack<EntityEntry> modifiedEntries,
        HashSet<object> visited,
        bool isAsync,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var existing = await queryable.FindExistingByUniqueKeyGenericAsync(entity, ignoreQueryFilters, ignoreAutoIncludes: true, tracking: true, cancellationToken: cancellationToken).ConfigureAwait(false);

        ProcessExistingEntity(context, entry, entity, existing, modifiedEntries, logger);

        await ProcessCascadeReferences(context, entity, entityModelPropertyMap, modifiedEntries, visited, isAsync, logger, cancellationToken).ConfigureAwait(false);
    }

    private static void ProcessExistingEntity(
        DbContext context,
        EntityEntry entry,
        ILogicallyDeletableEntityModel entity,
        object? existing,
        Stack<EntityEntry> modifiedEntries,
        ILogger logger)
    {
        if (existing is ILogicallyDeletableEntityModel existingEntity && existingEntity.IsLogicallyDeleted)
        {
            var existingEntry = context.Entry(existing) ??
                throw new InvalidOperationException($"Could not obtain change-tracking entry for existing entity '{existing.GetType().Name}'");

            EntityFrameworkLog.RecoveringLogicallyDeletedEntity(logger, entity.GetType().Name, EntityModelDbContextConventions.RecoverStrategy);

            switch (EntityModelDbContextConventions.RecoverStrategy)
            {
            case RecoverLogicallyDeletableStrategy.RecoverExisting:
                {
                    existingEntity.CopyEntityModelPropertiesFrom(entity, copyEntityModelIds: false, copyLogicallyDeletedFlags: true, copyEntityModelTimestamps: false);
                    existingEntry.State = EntityState.Modified;

                    modifiedEntries.Push(existingEntry);

                    entity.CopyEntityModelPropertiesFrom(existingEntity, copyEntityModelIds: true, copyLogicallyDeletedFlags: true, copyEntityModelTimestamps: true);
                    entry.State = EntityState.Detached;

                    break;
                }
            case RecoverLogicallyDeletableStrategy.DetachExistingAttachNew:
                {
                    DetachAllExistingEntries(context, entity, existingEntry);

                    entry.CopyEntityModelAndEntryIdsAndCreatedTimestampFrom(entity, existingEntity);
                    entry.State = EntityState.Modified;

                    modifiedEntries.Push(entry);

                    break;
                }
            default:
                {
                    throw new InvalidOperationException("Unsupported RecoverStrategy");
                }
            }
        }
    }

    private static ValueTask ProcessCascadeReferences(
        DbContext context,
        ILogicallyDeletableEntityModel entity,
        EntityModelPropertyMap entityModelPropertyMap,
        Stack<EntityEntry> modifiedEntries,
        HashSet<object> visited,
        bool isAsync,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (entity.IsLogicallyDeleted)
        {
            return ValueTask.CompletedTask;
        }

        if (entityModelPropertyMap.ForeignKeyNavigationReferences.Count == 0)
        {
            return ValueTask.CompletedTask;
        }

        if (isAsync)
        {
            return new ValueTask(ProcessCascadeReferencesAsync(context, entity, entityModelPropertyMap, modifiedEntries, visited, logger, cancellationToken));
        }

        foreach (var detailEntries in EnumerateCascadeDetailEntries(context, entity, entityModelPropertyMap, logger))
        {
            EnsureCompletedSynchronously(DoRecoverLogicallyDeletableEntitiesLoop(context, isAsync: false, detailEntries, modifiedEntries, visited, logger, cancellationToken));
        }

        return ValueTask.CompletedTask;
    }

    private static async Task ProcessCascadeReferencesAsync(
        DbContext context,
        ILogicallyDeletableEntityModel entity,
        EntityModelPropertyMap entityModelPropertyMap,
        Stack<EntityEntry> modifiedEntries,
        HashSet<object> visited,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        foreach (var detailEntries in EnumerateCascadeDetailEntries(context, entity, entityModelPropertyMap, logger))
        {
            await DoRecoverLogicallyDeletableEntitiesLoopAsync(context, isAsync: true, detailEntries, modifiedEntries, visited, logger, cancellationToken).ConfigureAwait(false);
        }
    }

    private static IEnumerable<List<EntityEntry>> EnumerateCascadeDetailEntries(
        DbContext context,
        ILogicallyDeletableEntityModel entity,
        EntityModelPropertyMap entityModelPropertyMap,
        ILogger logger)
    {
        foreach (var navigationForeignKeyReference in entityModelPropertyMap.ForeignKeyNavigationReferences)
        {
            if (!navigationForeignKeyReference.ForeignEntityModelType.Implements<ILogicallyDeletableEntityModel>())
            {
                continue;
            }

            var deleteBehaviour = navigationForeignKeyReference.ForeignKeyReference.DeleteBehavior ?? DeleteBehavior.NoAction;

            if ((deleteBehaviour == DeleteBehavior.NoAction) ||
                (deleteBehaviour == DeleteBehavior.ClientNoAction) ||
                (deleteBehaviour == DeleteBehavior.Restrict) ||
                (deleteBehaviour == DeleteBehavior.SetNull) ||
                (deleteBehaviour == DeleteBehavior.ClientSetNull))
            {
                continue;
            }
            else if ((deleteBehaviour == DeleteBehavior.Cascade) ||
                (deleteBehaviour == DeleteBehavior.ClientCascade))
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(navigationForeignKeyReference.ForeignKeyReference.ReverseNavigationPropertyName,
                    nameof(navigationForeignKeyReference.ForeignKeyReference.ReverseNavigationPropertyName));

                var propertyMetadata = entityModelPropertyMap.Properties.FirstOrDefault(
                    property => property.Name == navigationForeignKeyReference.ForeignKeyReference.ReverseNavigationPropertyName) ??
                    throw new InvalidOperationException($"Navigation property '{navigationForeignKeyReference.ForeignKeyReference.ReverseNavigationPropertyName}' not found on entity '{entityModelPropertyMap.EntityModelType?.Name}'");

                var navigationObject = propertyMetadata.PropertyInfo.GetValue(entity);

                if (navigationObject is not IEnumerable<ILogicallyDeletableEntityModel> navigation)
                {
                    continue;
                }

                var details = navigation.ToList();

                var detailEntries = details
                    .Select(detail => context.Entry(detail) as EntityEntry)
                    .Where(detailEntry => detailEntry.State == EntityState.Added)
                    .ToList();

                if (detailEntries.Count > 0)
                {
                    EntityFrameworkLog.CascadingLogicallyDeletedRecovery(logger, entity.GetType().Name, detailEntries.Count, propertyMetadata.Name);
                }

                yield return detailEntries;
            }
            else
            {
                throw new InvalidOperationException($"Invalid deleteBehaviour: {deleteBehaviour}");
            }
        }
    }

    private static void DetachAllExistingEntries(DbContext context, IEntityModel newEntity, EntityEntry existingEntry)
    {
        var entriesToDetach = new List<EntityEntry>()
        {
            existingEntry
        };

        var existingEntity = existingEntry.Entity;
        var entityPropertyMap = newEntity.GetEntityModelPropertyMap(context);
        var entityType = entityPropertyMap.EntityModelType;

        ArgumentNullException.ThrowIfNull(entityType, nameof(entityType));

        var keyProperties = context.Model.FindEntityType(entityType)!.FindPrimaryKey()!.Properties;

        if (keyProperties != null && keyProperties.Any())
        {
            var keyValues = keyProperties.Select(p => p.PropertyInfo!.GetValue(existingEntity)).ToArray();

            var conflictingEntries = context.ChangeTracker.Entries()
                .Where(e => e.Entity.GetType() == entityType && !ReferenceEquals(e.Entity, newEntity))
                .Where(e => keyProperties
                    .Select(p => p.PropertyInfo!.GetValue(e.Entity))
                    .SequenceEqual(keyValues))
                .ToList();

            foreach (var conflictingEntry in conflictingEntries)
            {
                entriesToDetach.Add(conflictingEntry);
            }
        }

        foreach (var entry in entriesToDetach)
        {
            entry.State = EntityState.Detached;

            if (entityPropertyMap.ForeignKeyReferences.Count == 0)
            {
                continue;
            }

            entry.RemoveEntityFromForeignKeyReferencesNavigations(context, entityPropertyMap);
        }
    }
}
