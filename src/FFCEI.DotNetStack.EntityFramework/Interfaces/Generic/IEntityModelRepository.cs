using FFCEI.DotNetStack.Models.Interfaces;
using System.Linq.Expressions;

namespace FFCEI.DotNetStack.EntityFramework.Interfaces.Generic;

public interface IEntityModelRepository<TEntityModel>
    : IReadOnlyEntityModelRepository<TEntityModel>, IEntityModelRepository where TEntityModel : class, IEntityModel
{
    Task AddNewAsync(TEntityModel entityModel, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task AddManyNewAsync(IEnumerable<TEntityModel> entityModels, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task UpdateExistingAsync(TEntityModel entityModel, bool overrideAllFields = false, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task UpdateManyExistingAsync(IEnumerable<TEntityModel> entityModels, bool overrideAllFields = false, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<TEntityModel?> DeleteExistingByKeyAsync(IEnumerable<object> keyValues, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TEntityModel>> DeleteManyExistingWhereAsync(Expression<Func<TEntityModel, bool>> predicate, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task DeleteExistingAsync(TEntityModel entityModel, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task DeleteManyExistingAsync(IEnumerable<TEntityModel> entityModels, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<TEntityModel?> LogicallyDeleteExistingByKeyAsync(IEnumerable<object> keyValues, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TEntityModel>> LogicallyDeleteManyExistingWhereAsync(Expression<Func<TEntityModel, bool>> predicate, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task LogicallyDeleteExistingAsync(TEntityModel entityModel, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task LogicallyDeleteManyExistingAsync(IEnumerable<TEntityModel> entityModels, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<TEntityModel?> LogicallyUndeleteExistingByKeyAsync(IEnumerable<object> keyValues, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TEntityModel>> LogicallyUndeleteManyExistingWhereAsync(Expression<Func<TEntityModel, bool>> predicate, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task LogicallyUndeleteExistingAsync(TEntityModel entityModel, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task LogicallyUndeleteManyExistingAsync(IEnumerable<TEntityModel> entityModels, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<TEntityModel?> PhysicallyDeleteExistingByKeyAsync(IEnumerable<object> keyValues, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TEntityModel>> PhysicallyDeleteManyExistingWhereAsync(Expression<Func<TEntityModel, bool>> predicate, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task<int> PhysicallyDeleteManyExistingDirectlyWhereAsync(Expression<Func<TEntityModel, bool>> predicate, CancellationToken cancellationToken = default);

    Task PhysicallyDeleteExistingAsync(TEntityModel entityModel, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task PhysicallyDeleteManyExistingAsync(IEnumerable<TEntityModel> entityModels, bool saveChanges = true, bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    Task ReloadExistingAsync(TEntityModel entityModel, CancellationToken cancellationToken = default);

    void DetachExisting(TEntityModel entityModel);
}
