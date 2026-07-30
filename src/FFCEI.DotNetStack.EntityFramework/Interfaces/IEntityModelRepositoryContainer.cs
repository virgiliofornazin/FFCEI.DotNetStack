using FFCEI.DotNetStack.EntityFramework.Interfaces.Generic;
using FFCEI.DotNetStack.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FFCEI.DotNetStack.EntityFramework.Interfaces;

public interface IEntityModelRepositoryContainer
    : IEntityModelDbContext, IDisposable
{
    DbContext DetachDbContext();

    IReadOnlyEntityModelRepository<TEntityModel> ReadOnlyRepository<TEntityModel>(ref IReadOnlyEntityModelRepository<TEntityModel>? repository)
        where TEntityModel : class, IEntityModel;

    IReadOnlyEntityModelRepository<TEntityModelInterface> ReadOnlyRepository<TEntityModel, TEntityModelInterface>(ref IReadOnlyEntityModelRepository<TEntityModelInterface>? repository)
        where TEntityModel : class, IEntityModel, TEntityModelInterface where TEntityModelInterface : class, IEntityModel;

    IEntityModelRepository<TEntityModel> Repository<TEntityModel>(ref IEntityModelRepository<TEntityModel>? repository)
        where TEntityModel : class, IEntityModel;

    IEntityModelRepository<TEntityModelInterface> Repository<TEntityModel, TEntityModelInterface>(ref IEntityModelRepository<TEntityModelInterface>? repository)
        where TEntityModel : class, IEntityModel, TEntityModelInterface where TEntityModelInterface : class, IEntityModel;

    void EnsureCreated();

    Task EnsureCreatedAsync(CancellationToken cancellationToken = default);

    void EnsureDeleted();

    Task EnsureDeletedAsync(CancellationToken cancellationToken = default);

    void Migrate();

    Task MigrateAsync(CancellationToken cancellationToken = default);
}
