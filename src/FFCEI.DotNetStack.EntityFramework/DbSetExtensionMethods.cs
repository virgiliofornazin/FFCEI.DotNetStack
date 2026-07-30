using Microsoft.EntityFrameworkCore;

namespace FFCEI.DotNetStack.EntityFramework;

public static class DbSetExtensionMethods
{
    public static Task<TEntityModel?> FindExistingByUniqueKeyAsync<TEntityModel>(this DbSet<TEntityModel> dbSet, TEntityModel entity,
        IReadOnlyCollection<string>? ignoreQueryFilters = null, bool? ignoreAutoIncludes = null, bool? tracking = null, CancellationToken cancellationToken = default) where TEntityModel : class
            => (dbSet as IQueryable<TEntityModel>).FindExistingByUniqueKeyAsync(entity, ignoreQueryFilters, ignoreAutoIncludes, tracking, cancellationToken);
}
