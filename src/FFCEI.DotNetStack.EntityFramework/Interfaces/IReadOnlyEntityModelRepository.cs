using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityFramework.Interfaces;

public interface IReadOnlyEntityModelRepository
    : IReadOnlyEntityModelDbContext
{
    ValueTask<IEntityModel?> FindModelByKeyAsync(params IEnumerable<object> keyValues);

    ValueTask<IEntityModel?> FindModelByKeyAsync(CancellationToken cancellationToken, params IEnumerable<object> keyValues);
}
