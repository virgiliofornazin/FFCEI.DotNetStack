using FFCEI.DotNetStack.EntityFramework.Interfaces.Generic;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace FFCEI.DotNetStack.EntityFramework;

public class EntityModelRepository<TEntityModel>(DbContext context)
    : EntityModelRepository<TEntityModel, TEntityModel>(context) where TEntityModel : class, IEntityModel
{
}

public class EntityModelRepository<TEntityModel, TEntityModelInterface>(DbContext context)
    : ReadOnlyEntityModelRepository<TEntityModel, TEntityModelInterface>(context), IEntityModelRepository<TEntityModelInterface> where TEntityModel : class, IEntityModel, TEntityModelInterface where TEntityModelInterface : class, IEntityModel
{
    private static IEnumerable<TEntityModel> FromEntityModelInterfaces(IEnumerable<TEntityModelInterface> models)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));

        foreach (var model in models)
        {
            if (model is not TEntityModel entityModel)
            {
                throw new InvalidCastException($"Cannot cast TEntityModelInterface ({(model?.GetType() ?? typeof(TEntityModelInterface)).FullName}) to TEntityModel ({typeof(TEntityModel).FullName})");
            }

            yield return entityModel;
        }
    }

    private static IEnumerable<TEntityModel> FromIEntityModel(IEnumerable<IEntityModel> models)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));

        foreach (var model in models)
        {
            if (model is not TEntityModel entityModel)
            {
                throw new InvalidCastException($"Cannot cast IEntityModel ({(model?.GetType() ?? typeof(TEntityModelInterface)).FullName}) to TEntityModel ({typeof(TEntityModel).FullName})");
            }

            yield return entityModel;
        }
    }

    private static IEnumerable<TEntityModel> AsLogicallyDeletable(IEnumerable<IEntityModel> models)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));

        foreach (var model in models)
        {
            if (model is not TEntityModel entityModel)
            {
                throw new InvalidCastException($"Cannot cast IEntityModel ({(model?.GetType() ?? typeof(TEntityModelInterface)).FullName}) to TEntityModel");
            }

            if (model is not ILogicallyDeletableEntityModel)
            {
                throw new InvalidCastException($"Cannot cast IEntityModel ({(model?.GetType() ?? typeof(TEntityModelInterface)).FullName}) to ILogicallyDeletableEntityModel");
            }

            yield return entityModel;
        }
    }

    public Task AddNewModelAsync(IEntityModel model, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        var entityModels = FromIEntityModel([model]);

        return AddManyNewAsync(entityModels, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
    }

    public Task AddManyNewModelsAsync(IEnumerable<IEntityModel> models, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        var entityModels = FromIEntityModel(models);

        return AddManyNewAsync(entityModels, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
    }

    public Task UpdateExistingModelAsync(IEntityModel model, bool overrideAllFields = false, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        var entityModels = FromIEntityModel([model]);

        return UpdateManyExistingAsync(entityModels, overrideAllFields, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
    }

    public Task UpdateManyExistingModelsAsync(IEnumerable<IEntityModel> models, bool overrideAllFields = false, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        var entityModels = FromIEntityModel(models);

        return UpdateManyExistingAsync(entityModels, overrideAllFields, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
    }

    public Task<IEntityModel?> DeleteExistingModelByKeyAsync(IEnumerable<object> keyValues, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        if (typeof(TEntityModel).Implements<ILogicallyDeletableEntityModel>())
        {
            return LogicallyDeleteExistingModelByKeyAsync(keyValues, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
        }
        else
        {
            return PhysicallyDeleteExistingModelByKeyAsync(keyValues, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
        }
    }

    public Task<IReadOnlyCollection<IEntityModel>> DeleteManyExistingModelsWhereAsync(Expression<Func<IEntityModel, bool>> predicate, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        if (typeof(TEntityModel).Implements<ILogicallyDeletableEntityModel>())
        {
            return LogicallyDeleteManyExistingModelsWhereAsync(predicate, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
        }
        else
        {
            return PhysicallyDeleteManyExistingModelsWhereAsync(predicate, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
        }
    }

    public Task DeleteExistingModelAsync(IEntityModel model, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        if (typeof(TEntityModel).Implements<ILogicallyDeletableEntityModel>())
        {
            return LogicallyDeleteExistingModelAsync(model, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
        }
        else
        {
            return PhysicallyDeleteExistingModelAsync(model, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
        }
    }

    public Task DeleteManyExistingModelsAsync(IEnumerable<IEntityModel> models, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        if (typeof(TEntityModel).Implements<ILogicallyDeletableEntityModel>())
        {
            return LogicallyDeleteManyExistingModelsAsync(models, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
        }
        else
        {
            return PhysicallyDeleteManyExistingModelsAsync(models, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
        }
    }

    public async Task<IEntityModel?> LogicallyDeleteExistingModelByKeyAsync(IEnumerable<object> keyValues, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keyValues, nameof(keyValues));

        var model = await FindModelByKeyAsync(cancellationToken, keyValues).ConfigureAwait(false);

        if (model != null)
        {
            await LogicallyDeleteExistingModelAsync(model, saveChanges, acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
        }

        return model;
    }

    public async Task<IReadOnlyCollection<IEntityModel>> LogicallyDeleteManyExistingModelsWhereAsync(Expression<Func<IEntityModel, bool>> predicate, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        var models = await CheckedSet.Where(predicate).ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        if (models.Count != 0)
        {
            await LogicallyDeleteManyExistingModelsAsync(models, saveChanges, acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
        }

        return models;
    }

    public Task LogicallyDeleteExistingModelAsync(IEntityModel model, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
        => LogicallyDeleteManyExistingModelsAsync([model], saveChanges, acceptAllChangesOnSuccess, cancellationToken);

    public Task LogicallyDeleteManyExistingModelsAsync(IEnumerable<IEntityModel> models, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        var entityModels = AsLogicallyDeletable(models);

        return LogicallyDeleteManyExistingAsync(entityModels, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
    }

    public async Task<IEntityModel?> LogicallyUndeleteExistingModelByKeyAsync(IEnumerable<object> keyValues, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keyValues, nameof(keyValues));

        var model = await FindModelByKeyAsync(cancellationToken, keyValues).ConfigureAwait(false);

        if (model != null)
        {
            await LogicallyUndeleteExistingModelAsync(model, saveChanges, acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
        }

        return model;
    }

    public async Task<IReadOnlyCollection<IEntityModel>> LogicallyUndeleteManyExistingModelsWhereAsync(Expression<Func<IEntityModel, bool>> predicate, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        var models = await CheckedSet.Where(predicate).ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        if (models.Count != 0)
        {
            await LogicallyUndeleteManyExistingModelsAsync(models, saveChanges, acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
        }

        return models;
    }

    public Task LogicallyUndeleteExistingModelAsync(IEntityModel model, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
        => LogicallyUndeleteManyExistingModelsAsync([model], saveChanges, acceptAllChangesOnSuccess, cancellationToken);

    public Task LogicallyUndeleteManyExistingModelsAsync(IEnumerable<IEntityModel> models, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        var entityModels = AsLogicallyDeletable(models);

        return LogicallyUndeleteManyExistingAsync(entityModels, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
    }

    public async Task<IEntityModel?> PhysicallyDeleteExistingModelByKeyAsync(IEnumerable<object> keyValues, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keyValues, nameof(keyValues));

        var model = await FindModelByKeyAsync(cancellationToken, keyValues).ConfigureAwait(false);

        if (model != null)
        {
            await PhysicallyDeleteExistingModelAsync(model, saveChanges, acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
        }

        return model;
    }

    public async Task<IReadOnlyCollection<IEntityModel>> PhysicallyDeleteManyExistingModelsWhereAsync(Expression<Func<IEntityModel, bool>> predicate, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        var models = await CheckedSet.Where(predicate).ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        if (models.Count != 0)
        {
            await PhysicallyDeleteManyExistingModelsAsync(models, saveChanges, acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
        }

        return models;
    }

    public async Task<int> PhysicallyDeleteManyExistingModelsDirectlyWhereAsync(Expression<Func<IEntityModel, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var result = await CheckedSet.Where(predicate).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

        return result;
    }

    public Task PhysicallyDeleteExistingModelAsync(IEntityModel model, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
        => PhysicallyDeleteManyExistingModelsAsync([model], saveChanges, acceptAllChangesOnSuccess, cancellationToken);

    public Task PhysicallyDeleteManyExistingModelsAsync(IEnumerable<IEntityModel> models, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        var entityModels = FromIEntityModel(models);

        return PhysicallyDeleteManyExistingAsync(entityModels, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
    }

    public Task ReloadExistingModelAsync(IEntityModel model, CancellationToken cancellationToken = default)
    {
        var entityModels = FromIEntityModel([model]);

        return ReloadExistingAsync(entityModels.First(), cancellationToken);
    }

    public void DetachExistingModel(IEntityModel model)
    {
        var entityModels = FromIEntityModel([model]);

        DetachExisting(entityModels.First());
    }

    public Task AddNewAsync(TEntityModelInterface entityModel, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
        => AddManyNewAsync([entityModel], saveChanges, acceptAllChangesOnSuccess, cancellationToken);

    public async Task AddManyNewAsync(IEnumerable<TEntityModelInterface> entityModels, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        var models = FromEntityModelInterfaces(entityModels);

        CheckedSet.AddRange(models);

        if (saveChanges)
        {
            _ = await SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
        }
    }

    public Task UpdateExistingAsync(TEntityModelInterface entityModel, bool overrideAllFields = false, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
        => UpdateManyExistingAsync([entityModel], overrideAllFields, saveChanges, acceptAllChangesOnSuccess, cancellationToken);

    public async Task UpdateManyExistingAsync(IEnumerable<TEntityModelInterface> entityModels, bool overrideAllFields = false, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        var models = FromEntityModelInterfaces(entityModels);

        if (overrideAllFields)
        {
            CheckedSet.UpdateRange(models);
        }
        else
        {
            foreach (var model in models)
            {
                var entry = CheckedContext.Entry(model);

                if (entry.State == EntityState.Detached)
                {
                    _ = CheckedSet.Update(model);
                }
            }
        }

        if (saveChanges)
        {
            _ = await SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
        }
    }

    public Task<TEntityModelInterface?> DeleteExistingByKeyAsync(IEnumerable<object> keyValues, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        if (typeof(TEntityModel).Implements<ILogicallyDeletableEntityModel>())
        {
            return LogicallyDeleteExistingByKeyAsync(keyValues, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
        }
        else
        {
            return PhysicallyDeleteExistingByKeyAsync(keyValues, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
        }
    }

    public Task<IReadOnlyCollection<TEntityModelInterface>> DeleteManyExistingWhereAsync(Expression<Func<TEntityModelInterface, bool>> predicate, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        if (typeof(TEntityModel).Implements<ILogicallyDeletableEntityModel>())
        {
            return LogicallyDeleteManyExistingWhereAsync(predicate, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
        }
        else
        {
            return PhysicallyDeleteManyExistingWhereAsync(predicate, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
        }
    }

    public Task DeleteExistingAsync(TEntityModelInterface entityModel, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        if (typeof(TEntityModel).Implements<ILogicallyDeletableEntityModel>())
        {
            return LogicallyDeleteExistingAsync(entityModel, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
        }
        else
        {
            return PhysicallyDeleteExistingAsync(entityModel, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
        }
    }

    public Task DeleteManyExistingAsync(IEnumerable<TEntityModelInterface> entityModels, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        if (typeof(TEntityModel).Implements<ILogicallyDeletableEntityModel>())
        {
            return LogicallyDeleteManyExistingAsync(entityModels, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
        }
        else
        {
            return PhysicallyDeleteManyExistingAsync(entityModels, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
        }
    }

    public async Task<TEntityModelInterface?> LogicallyDeleteExistingByKeyAsync(IEnumerable<object> keyValues, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keyValues, nameof(keyValues));

        var entityModel = await FindByKeyAsync(cancellationToken, keyValues).ConfigureAwait(false);

        if (entityModel != null)
        {
            await LogicallyDeleteManyExistingAsync([entityModel], saveChanges, acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
        }

        return entityModel;
    }

    public async Task<IReadOnlyCollection<TEntityModelInterface>> LogicallyDeleteManyExistingWhereAsync(Expression<Func<TEntityModelInterface, bool>> predicate, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        var entityModels = await CheckedSet.Where(predicate).ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        if (entityModels.Count != 0)
        {
            await LogicallyDeleteManyExistingAsync(entityModels, saveChanges, acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
        }

        return entityModels;
    }

    public Task LogicallyDeleteExistingAsync(TEntityModelInterface entityModel, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
        => LogicallyDeleteManyExistingAsync([entityModel], saveChanges, acceptAllChangesOnSuccess, cancellationToken);

    public Task LogicallyDeleteManyExistingAsync(IEnumerable<TEntityModelInterface> entityModels, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        var models = AsLogicallyDeletable(entityModels).ToList();

        foreach (var model in models)
        {
            if (model is ILogicallyDeletableEntityModel castedModel)
            {
                castedModel.IsLogicallyDeleted = true;
                castedModel.WasLogicallyDeletedCascaded = false;
            }
        }

        return UpdateManyExistingAsync(models, overrideAllFields: false, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
    }

    public async Task<TEntityModelInterface?> LogicallyUndeleteExistingByKeyAsync(IEnumerable<object> keyValues, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keyValues, nameof(keyValues));

        var entityModel = await FindByKeyAsync(cancellationToken, keyValues).ConfigureAwait(false);

        if (entityModel != null)
        {
            await LogicallyUndeleteManyExistingAsync([entityModel], saveChanges, acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
        }

        return entityModel;
    }

    public async Task<IReadOnlyCollection<TEntityModelInterface>> LogicallyUndeleteManyExistingWhereAsync(Expression<Func<TEntityModelInterface, bool>> predicate, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        var entityModels = await CheckedSet.Where(predicate).ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        if (entityModels.Count != 0)
        {
            await LogicallyUndeleteManyExistingAsync(entityModels, saveChanges, acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
        }

        return entityModels;
    }

    public Task LogicallyUndeleteExistingAsync(TEntityModelInterface entityModel, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
        => LogicallyUndeleteManyExistingAsync([entityModel], saveChanges, acceptAllChangesOnSuccess, cancellationToken);

    public Task LogicallyUndeleteManyExistingAsync(IEnumerable<TEntityModelInterface> entityModels, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        var models = AsLogicallyDeletable(entityModels).ToList();

        foreach (var model in models)
        {
            if (model is ILogicallyDeletableEntityModel castedModel)
            {
                castedModel.IsLogicallyDeleted = false;
                castedModel.WasLogicallyDeletedCascaded = false;
            }
        }

        return UpdateManyExistingAsync(models, overrideAllFields: false, saveChanges, acceptAllChangesOnSuccess, cancellationToken);
    }

    public async Task<TEntityModelInterface?> PhysicallyDeleteExistingByKeyAsync(IEnumerable<object> keyValues, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keyValues, nameof(keyValues));

        var entityModel = await FindByKeyAsync(cancellationToken, keyValues).ConfigureAwait(false);

        if (entityModel != null)
        {
            await PhysicallyDeleteManyExistingAsync([entityModel], saveChanges, acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
        }

        return entityModel;
    }

    public async Task<IReadOnlyCollection<TEntityModelInterface>> PhysicallyDeleteManyExistingWhereAsync(Expression<Func<TEntityModelInterface, bool>> predicate, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        var entityModels = await CheckedSet.Where(predicate).ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        if (entityModels.Count != 0)
        {
            await PhysicallyDeleteManyExistingAsync(entityModels, saveChanges, acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
        }

        return entityModels;
    }

    public async Task<int> PhysicallyDeleteManyExistingDirectlyWhereAsync(Expression<Func<TEntityModelInterface, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var result = await CheckedSet.Where(predicate).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

        return result;
    }

    public Task PhysicallyDeleteExistingAsync(TEntityModelInterface entityModel, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
        => PhysicallyDeleteManyExistingAsync([entityModel], saveChanges, acceptAllChangesOnSuccess, cancellationToken);

    public async Task PhysicallyDeleteManyExistingAsync(IEnumerable<TEntityModelInterface> entityModels, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
    {
        var models = FromEntityModelInterfaces(entityModels);

        foreach (var model in models)
        {
            var entry = CheckedContext.Entry(model);

            if (entry.State != EntityState.Detached)
            {
                _ = CheckedSet.Remove(model);
            }
        }

        if (saveChanges)
        {
            _ = await SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
        }
    }

    public Task ReloadExistingAsync(TEntityModelInterface entityModel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entityModel, nameof(entityModel));

        return CheckedContext.Entry(entityModel).ReloadAsync(cancellationToken);
    }

    public void DetachExisting(TEntityModelInterface entityModel)
    {
        ArgumentNullException.ThrowIfNull(entityModel, nameof(entityModel));

        CheckedContext.Entry(entityModel).State = EntityState.Detached;
    }

    public int SaveChanges()
        => CheckedContext.SaveChanges();

    public int SaveChanges(bool acceptAllChangesOnSuccess)
        => CheckedContext.SaveChanges(acceptAllChangesOnSuccess);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => CheckedContext.SaveChangesAsync(cancellationToken);

    public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        => CheckedContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

    public ChangeTracker ChangeTracker
        => CheckedContext.ChangeTracker;

    public int ExecuteSqlRaw(string sql, params IEnumerable<object?> parameters)
        => CheckedContext.Database.ExecuteSqlRaw(sql, [.. parameters]);

    public Task<int> ExecuteSqlRawAsync(string sql, params IEnumerable<object> parameters)
        => CheckedContext.Database.ExecuteSqlRawAsync(sql, [.. parameters]);

    public Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters, CancellationToken cancellationToken)
        => CheckedContext.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
}
