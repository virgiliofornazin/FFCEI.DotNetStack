using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Models;
using FFCEI.DotNetStack.Models.Interfaces;
using FFCEI.DotNetStack.Models.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace FFCEI.DotNetStack.EntityFramework;

public static class IQueryableExtensionMethods
{
    public static IQueryable<TEntityModel> SkipTakeByPage<TEntityModel>(this IQueryable<TEntityModel> queryable, int? pageIndex = null, int? pageSize = null, bool forcePaging = false) where TEntityModel : class
    {
        if (PagingAlgorithms.ComputeSkipTake(pageIndex, pageSize, out var skip, out var take, forcePaging) && skip.HasValue && take.HasValue)
        {
            return queryable.Skip(skip.Value).Take(take.Value);
        }

        return queryable;
    }

    public static IQueryable<TEntityModel> IgnoreLogicallyDeletedQueryFilter<TEntityModel>(this IQueryable<TEntityModel> queryable) where TEntityModel : class
        => queryable.IgnoreQueryFilters([QueryFilters.IsLogicallyDeletedQueryFilterName]);

    private static bool BuildFindByUniqueKeyExpressions<TEntityModel>(IQueryable<TEntityModel> queryable, TEntityModel entity,
        IReadOnlyCollection<string>? ignoreQueryFilters, bool? ignoreAutoIncludes, bool? tracking, out IQueryable<TEntityModel> query,
            out List<Expression<Func<TEntityModel, bool>>>? lambdas) where TEntityModel : class
    {
        ArgumentNullException.ThrowIfNull(queryable, nameof(queryable));
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        query = queryable;
        lambdas = null;

        var entityPropertyMap = EntityModelPropertyMapCache.Instance.GetOrAdd(entity.GetType());

        if (entityPropertyMap.UniqueKeys.Count == 0)
        {
            return false;
        }

        if (ignoreQueryFilters != null)
        {
            if (ignoreQueryFilters.Count == 0)
            {
                query = query.IgnoreQueryFilters();
            }
            else
            {
                query = query.IgnoreQueryFilters(ignoreQueryFilters);
            }
        }

        if (ignoreAutoIncludes.HasValue && ignoreAutoIncludes.Value)
        {
            query = query.IgnoreAutoIncludes();
        }

        if (tracking.HasValue)
        {
            query = tracking.Value ? query.AsTracking() : query.AsNoTracking();
        }

        foreach (var uniqueKey in entityPropertyMap.UniqueKeys)
        {
            var nullValueOnKey = false;
            var parameter = Expression.Parameter(typeof(TEntityModel), "e");

            Expression? combinedBody = null;

            foreach (var columnProperty in uniqueKey.Properties)
            {
                var property = columnProperty.PropertyMetadata.PropertyInfo;
                var entityValue = property.GetValue(entity);

                if (entityValue == null)
                {
                    nullValueOnKey = true;

                    break;
                }

                var resolvedProperty = parameter.Type.GetProperty(property.Name) ?? property;
                var left = Expression.Property(parameter, resolvedProperty);
                var right = Expression.Constant(entityValue, property.PropertyType);
                var equality = Expression.Equal(left, right);

                combinedBody = combinedBody == null ? equality : Expression.AndAlso(combinedBody, equality);
            }

            if (nullValueOnKey || (combinedBody == null))
            {
                continue;
            }

            var lambda = Expression.Lambda<Func<TEntityModel, bool>>(combinedBody, parameter);

            lambdas ??= [];
            lambdas.Add(lambda);
        }

        return (lambdas != null);
    }

    public static TEntityModel? FindExistingByUniqueKey<TEntityModel>(this IQueryable<TEntityModel> queryable, TEntityModel entity,
        IReadOnlyCollection<string>? ignoreQueryFilters = null, bool? ignoreAutoIncludes = null, bool? tracking = null) where TEntityModel : class
    {
        var findable = BuildFindByUniqueKeyExpressions(queryable, entity, ignoreQueryFilters, ignoreAutoIncludes, tracking, out var query, out var lambdas);

        if (!findable)
        {
            return null;
        }

        foreach (var lambda in lambdas ?? [])
        {
            var foundEntity = query.FirstOrDefault(lambda);

            if (foundEntity != null)
            {
                return foundEntity;
            }
        }

        return null;
    }

    public static async Task<TEntityModel?> FindExistingByUniqueKeyAsync<TEntityModel>(this IQueryable<TEntityModel> queryable, TEntityModel entity,
        IReadOnlyCollection<string>? ignoreQueryFilters = null, bool? ignoreAutoIncludes = null, bool? tracking = null, CancellationToken cancellationToken = default) where TEntityModel : class
    {
        var findable = BuildFindByUniqueKeyExpressions(queryable, entity, ignoreQueryFilters, ignoreAutoIncludes, tracking, out var query, out var lambdas);

        if (!findable)
        {
            return null;
        }

        foreach (var lambda in lambdas ?? [])
        {
            var foundEntity = await query.FirstOrDefaultAsync(lambda, cancellationToken).ConfigureAwait(false);

            if (foundEntity != null)
            {
                return foundEntity;
            }
        }

        return null;
    }

    private static readonly MethodInfo? _methodIQueryableExtensionMethodsFindExistingByUniqueKey = typeof(IQueryableExtensionMethods)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .FirstOrDefault(m => m.Name == nameof(FindExistingByUniqueKey) && m.IsGenericMethod);

    public static IEntityModel? FindExistingByUniqueKeyGeneric(this IQueryable queryable, object entityModel, IReadOnlyCollection<string>? ignoreQueryFilters = null, bool? ignoreAutoIncludes = null, bool? tracking = null)
    {
        ArgumentNullException.ThrowIfNull(queryable, nameof(queryable));
        ArgumentNullException.ThrowIfNull(entityModel, nameof(entityModel));

        if (_methodIQueryableExtensionMethodsFindExistingByUniqueKey == null)
        {
            throw new InvalidOperationException($"Method '{nameof(FindExistingByUniqueKey)}' not found by reflection; incompatible library version");
        }

        var entityModelType = entityModel.GetType().GetLazyLoadingProxyUnderlyingType();

        var result = _methodIQueryableExtensionMethodsFindExistingByUniqueKey
            .MakeGenericMethod(entityModelType)
            .Invoke(null, [queryable, entityModel, ignoreQueryFilters, ignoreAutoIncludes, tracking]) as IEntityModel;

        return result;
    }

    private static readonly MethodInfo? _methodIQueryableExtensionMethodsFindExistingByUniqueKeyAsync = typeof(IQueryableExtensionMethods)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .FirstOrDefault(m => m.Name == nameof(FindExistingByUniqueKeyAsync) && m.IsGenericMethod);

    public static async Task<IEntityModel?> FindExistingByUniqueKeyGenericAsync(this IQueryable queryable, object entityModel,
        IReadOnlyCollection<string>? ignoreQueryFilters = null, bool? ignoreAutoIncludes = null, bool? tracking = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(queryable, nameof(queryable));
        ArgumentNullException.ThrowIfNull(entityModel, nameof(entityModel));

        if (_methodIQueryableExtensionMethodsFindExistingByUniqueKeyAsync == null)
        {
            throw new InvalidOperationException($"Method '{nameof(FindExistingByUniqueKeyAsync)}' not found by reflection; incompatible library version");
        }

        var entityModelType = entityModel.GetType().GetLazyLoadingProxyUnderlyingType();

        var taskResult = _methodIQueryableExtensionMethodsFindExistingByUniqueKeyAsync
            .MakeGenericMethod(entityModelType)
            .Invoke(null, [queryable, entityModel, ignoreQueryFilters, ignoreAutoIncludes, tracking, cancellationToken]);

        if (taskResult is Task task)
        {
            await task.ConfigureAwait(false);

            return task.GetType().GetProperty("Result")?.GetValue(task) as IEntityModel;
        }

        return null;
    }
}
