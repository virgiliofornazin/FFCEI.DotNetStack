using FFCEI.DotNetStack.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Reflection;

namespace FFCEI.DotNetStack.EntityFramework.Infrastructure;

public sealed class EntityModelDbContextModelCacheKeyFactory
    : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime) => (Type: context?.GetType() ?? typeof(DbContext),
        DatabaseProviderType: context is IEntityModelDbContext entityModelDbContext ? entityModelDbContext.DatabaseProvider : DatabaseProviderType.Unknown,
        RootAssembly: (context as EntityModelDbContext)?.RootAssembly ?? Assembly.GetEntryAssembly(), DesignTime: designTime);
}
