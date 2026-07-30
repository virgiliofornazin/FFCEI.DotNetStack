using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.EntityFramework.Interfaces.Generic;

public interface IReadOnlyEntityModelRepository<TEntityModel>
    : IReadOnlyEntityModelRepository, IQueryable<TEntityModel> where TEntityModel : class, IEntityModel
{
    ValueTask<TEntityModel?> FindByKeyAsync(params IEnumerable<object> keyValues);

    ValueTask<TEntityModel?> FindByKeyAsync(CancellationToken cancellationToken, params IEnumerable<object> keyValues);
}
