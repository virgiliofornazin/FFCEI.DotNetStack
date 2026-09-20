using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.EntityFramework.Interceptors;

public sealed class LogicallyDeletableEntitiesForeignKeyDeleteBehaviourSaveChangesInterceptor
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));

        LogicallyDeletableEntitiesForeignKeyDeleteBehaviour(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));

        LogicallyDeletableEntitiesForeignKeyDeleteBehaviour(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void LogicallyDeletableEntitiesForeignKeyDeleteBehaviour(DbContext? context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var entries = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList();

        if (entries.Count == 0)
        {
            return;
        }

        var logger = context.GetService<ILoggerFactory>().CreateLogger<LogicallyDeletableEntitiesForeignKeyDeleteBehaviourSaveChangesInterceptor>();
        var modifiedEntries = new Stack<EntityEntry>();
        var lazyLoadingEnabled = context.ChangeTracker.LazyLoadingEnabled;
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var deletingRoots = new List<ILogicallyDeletableEntityModel>();
        var restoringRoots = new List<ILogicallyDeletableEntityModel>();

        foreach (var entry in entries)
        {
            if (entry.Entity is ILogicallyDeletableEntityModel entity)
            {
                (entity.IsLogicallyDeleted ? deletingRoots : restoringRoots).Add(entity);
            }
        }

        foreach (var entity in deletingRoots)
        {
            ProcessForeignKeyDeleteBehaviour(context, entity, modifiedEntries, visited, logger);
        }

        foreach (var entity in restoringRoots)
        {
            ProcessForeignKeyDeleteBehaviour(context, entity, modifiedEntries, visited, logger);
        }

        try
        {
            context.ChangeTracker.LazyLoadingEnabled = false;

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

    private static void ProcessForeignKeyDeleteBehaviour(
        DbContext context,
        ILogicallyDeletableEntityModel entity,
        Stack<EntityEntry> modifiedEntries,
        HashSet<object> visited,
        ILogger logger)
    {
        if (!visited.Add(entity))
        {
            return;
        }

        var entityPropertyMap = entity.GetEntityModelPropertyMap(context);

        if (entity.IsLogicallyDeleted)
        {
            entity.RemoveEntityFromForeignKeyReferencesNavigations(context, entityPropertyMap);
        }

        foreach (var navigationForeignKeyReference in entityPropertyMap.ForeignKeyNavigationReferences)
        {
            if (!navigationForeignKeyReference.ForeignEntityModelType.Implements<ILogicallyDeletableEntityModel>())
            {
                continue;
            }

            var deleteBehaviour = navigationForeignKeyReference.ForeignKeyReference.DeleteBehavior ?? DeleteBehavior.NoAction;

            if ((deleteBehaviour == DeleteBehavior.NoAction) ||
                (deleteBehaviour == DeleteBehavior.ClientNoAction))
            {
                continue;
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(navigationForeignKeyReference.ForeignKeyReference.ReverseNavigationPropertyName,
                nameof(navigationForeignKeyReference.ForeignKeyReference.ReverseNavigationPropertyName));

            var propertyMetadata = entityPropertyMap.Properties.FirstOrDefault(
                property => property.Name == navigationForeignKeyReference.ForeignKeyReference.ReverseNavigationPropertyName) ??
                throw new InvalidOperationException($"Navigation property '{navigationForeignKeyReference.ForeignKeyReference.ReverseNavigationPropertyName}' not found on entity '{entityPropertyMap.EntityModelType.Name}'");

            var navigationObject = propertyMetadata.PropertyInfo.GetValue(entity);

            if (navigationObject is not IEnumerable<ILogicallyDeletableEntityModel> navigation)
            {
                continue;
            }

            var details = navigation.ToList();

            if (entity.IsLogicallyDeleted)
            {
                if (deleteBehaviour == DeleteBehavior.Restrict)
                {
                    if (details.Any(detail => !detail.IsLogicallyDeleted))
                    {
                        throw new InvalidOperationException($"You have a restrict deleteBehaviour clause and your navigation property {navigationForeignKeyReference.ForeignKeyReference.ReverseNavigationPropertyName} has dependent valid entities on your {entityPropertyMap.EntityModelType.Name} entity");
                    }
                }
                else if ((deleteBehaviour == DeleteBehavior.Cascade) ||
                    (deleteBehaviour == DeleteBehavior.ClientCascade) ||
                    (deleteBehaviour == DeleteBehavior.SetNull) ||
                    (deleteBehaviour == DeleteBehavior.ClientSetNull))
                {
                    foreach (var detail in details)
                    {
                        if (detail == null)
                        {
                            throw new InvalidOperationException($"Navigation property '{navigationForeignKeyReference.ForeignKeyReference.ReverseNavigationPropertyName}' on entity '{entityPropertyMap.EntityModelType.Name}' contains a null entity");
                        }

                        var detailEntry = context.Entry(detail);

                        if ((deleteBehaviour == DeleteBehavior.Cascade) ||
                            (deleteBehaviour == DeleteBehavior.ClientCascade))
                        {
                            if (!detail.IsLogicallyDeleted)
                            {
                                detail.IsLogicallyDeleted = true;
                                detail.WasLogicallyDeletedCascaded = true;

                                if (detailEntry.State == EntityState.Unchanged)
                                {
                                    detailEntry.State = EntityState.Modified;

                                    modifiedEntries.Push(detailEntry);
                                }

                                EntityFrameworkLog.CascadedLogicalDelete(logger, entityPropertyMap.EntityModelType.Name, detail.GetType().Name,
                                    navigationForeignKeyReference.ForeignKeyReference.ReverseNavigationPropertyName);

                                ProcessForeignKeyDeleteBehaviour(context, detail, modifiedEntries, visited, logger);
                            }
                        }
                        else if ((deleteBehaviour == DeleteBehavior.SetNull) ||
                            (deleteBehaviour == DeleteBehavior.ClientSetNull))
                        {
                            var property = navigationForeignKeyReference.ForeignKeyReference.EntityProperty;
                            property?.SetValue(detail, null);

                            property = navigationForeignKeyReference.ForeignKeyReference.DatabaseIdProperty;
                            property?.SetValue(detail, property.PropertyType.IsNullable() ? null : default(long));

                            property = navigationForeignKeyReference.ForeignKeyReference.UniversalUniqueIdProperty;
                            property?.SetValue(detail, property.PropertyType.IsNullable() ? null : default(Guid));

                            if (detailEntry.State == EntityState.Unchanged)
                            {
                                detailEntry.State = EntityState.Modified;

                                modifiedEntries.Push(detailEntry);
                            }

                            EntityFrameworkLog.ClearedForeignKeyOnLogicalDelete(logger, entityPropertyMap.EntityModelType.Name, detail.GetType().Name,
                                navigationForeignKeyReference.ForeignKeyReference.ReverseNavigationPropertyName);
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Invalid deleteBehaviour: {deleteBehaviour}");
                }
            }
            else if (!entity.IsLogicallyDeleted && entity.WasLogicallyDeletedCascaded)
            {
                if ((deleteBehaviour == DeleteBehavior.Restrict) ||
                    (deleteBehaviour == DeleteBehavior.SetNull) ||
                    (deleteBehaviour == DeleteBehavior.ClientSetNull))
                {
                    continue;
                }
                else if ((deleteBehaviour == DeleteBehavior.Cascade) ||
                    (deleteBehaviour == DeleteBehavior.ClientCascade))
                {
                    foreach (var detail in details)
                    {
                        if (detail == null)
                        {
                            throw new InvalidOperationException($"Navigation property '{navigationForeignKeyReference.ForeignKeyReference.ReverseNavigationPropertyName}' on entity '{entityPropertyMap.EntityModelType.Name}' contains a null entity");
                        }

                        var detailEntry = context.Entry(detail);

                        var wasDetailLogicallyDeletedCascaded = detailEntry.State != EntityState.Modified ? null :
                            detailEntry.Property(nameof(ILogicallyDeletableEntityModel.WasLogicallyDeletedCascaded)).OriginalValue as bool?;

                        if (((wasDetailLogicallyDeletedCascaded ?? false) || detail.WasLogicallyDeletedCascaded) && detail.IsLogicallyDeleted)
                        {
                            detail.IsLogicallyDeleted = false;
                            detail.WasLogicallyDeletedCascaded = false;

                            if (detailEntry.State == EntityState.Unchanged)
                            {
                                detailEntry.State = EntityState.Modified;

                                modifiedEntries.Push(detailEntry);
                            }

                            EntityFrameworkLog.CascadedLogicalDeleteUndo(logger, entityPropertyMap.EntityModelType.Name, detail.GetType().Name,
                                navigationForeignKeyReference.ForeignKeyReference.ReverseNavigationPropertyName);

                            ProcessForeignKeyDeleteBehaviour(context, detail, modifiedEntries, visited, logger);
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Invalid deleteBehaviour: {deleteBehaviour}");
                }
            }
        }
    }
}
