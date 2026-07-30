using FFCEI.DotNetStack.Models.Interfaces;
using System.Linq.Expressions;

namespace FFCEI.DotNetStack.EntityFramework.Interfaces;

public interface IEntityModelRepository
    : IReadOnlyEntityModelRepository, IEntityModelDbContext
{
    Task AddNewModelAsync(IEntityModel model, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task AddManyNewModelsAsync(IEnumerable<IEntityModel> models, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task UpdateExistingModelAsync(IEntityModel model, bool overrideAllFields = false, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task UpdateManyExistingModelsAsync(IEnumerable<IEntityModel> models, bool overrideAllFields = false, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<IEntityModel?> DeleteExistingModelByKeyAsync(IEnumerable<object> keyValues, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<IEntityModel>> DeleteManyExistingModelsWhereAsync(Expression<Func<IEntityModel, bool>> predicate, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task DeleteExistingModelAsync(IEntityModel model, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task DeleteManyExistingModelsAsync(IEnumerable<IEntityModel> models, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<IEntityModel?> LogicallyDeleteExistingModelByKeyAsync(IEnumerable<object> keyValues, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<IEntityModel>> LogicallyDeleteManyExistingModelsWhereAsync(Expression<Func<IEntityModel, bool>> predicate, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task LogicallyDeleteExistingModelAsync(IEntityModel model, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task LogicallyDeleteManyExistingModelsAsync(IEnumerable<IEntityModel> models, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<IEntityModel?> LogicallyUndeleteExistingModelByKeyAsync(IEnumerable<object> keyValues, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<IEntityModel>> LogicallyUndeleteManyExistingModelsWhereAsync(Expression<Func<IEntityModel, bool>> predicate, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task LogicallyUndeleteExistingModelAsync(IEntityModel model, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task LogicallyUndeleteManyExistingModelsAsync(IEnumerable<IEntityModel> models, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<IEntityModel?> PhysicallyDeleteExistingModelByKeyAsync(IEnumerable<object> keyValues, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<IEntityModel>> PhysicallyDeleteManyExistingModelsWhereAsync(Expression<Func<IEntityModel, bool>> predicate, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<int> PhysicallyDeleteManyExistingModelsDirectlyWhereAsync(Expression<Func<IEntityModel, bool>> predicate, CancellationToken cancellationToken = default);

    Task PhysicallyDeleteExistingModelAsync(IEntityModel model, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task PhysicallyDeleteManyExistingModelsAsync(IEnumerable<IEntityModel> models, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task ReloadExistingModelAsync(IEntityModel model, CancellationToken cancellationToken = default);

    void DetachExistingModel(IEntityModel model);
}
