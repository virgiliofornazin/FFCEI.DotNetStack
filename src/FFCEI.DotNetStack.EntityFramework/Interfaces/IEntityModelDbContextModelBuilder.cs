using Microsoft.EntityFrameworkCore;

namespace FFCEI.DotNetStack.EntityFramework.Interfaces;

public interface IEntityModelDbContextModelBuilder
{
    static abstract void BuildModel(ModelBuilder modelBuilder, DatabaseProviderType databaseProvider);
}
