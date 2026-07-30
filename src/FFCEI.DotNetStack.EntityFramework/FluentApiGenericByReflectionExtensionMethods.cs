using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;
using System.Reflection;

namespace FFCEI.DotNetStack.EntityFramework;

public static class FluentApiGenericByReflectionExtensionMethods
{
    private static readonly MethodInfo? _methodModelBuilderEntity = typeof(ModelBuilder)
        .GetMethods()
        .FirstOrDefault(m => m.Name == nameof(ModelBuilder.Entity) && m.IsGenericMethod && m.GetParameters().Length == 0);

    public static EntityTypeBuilder EntityGeneric(this ModelBuilder modelBuilder, Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType, nameof(entityType));

        if (_methodModelBuilderEntity == null)
        {
            throw new InvalidOperationException("Method 'ModelBuilder.Entity<T>()' not found by reflection; incompatible EF Core version");
        }

        var entity = _methodModelBuilderEntity
            .MakeGenericMethod(entityType)
            .Invoke(modelBuilder, null) as EntityTypeBuilder ??
            throw new InvalidOperationException($"Could not create EntityTypeBuilder for entity type '{entityType.Name}'");

        return entity;
    }

    public static PropertyBuilder PropertyGeneric(this EntityTypeBuilder entity, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var entityType = entity.Metadata.ClrType;
        var entityTypeBuilderGenericType = typeof(EntityTypeBuilder<>).MakeGenericType(entityType);

        var (propertyLambdaType, propertyLambda) = entityType.LambdaExpressionGenericEntityProperty(propertyName);

        var propertyBuilder = entityTypeBuilderGenericType
            .GetMethods()
            .First(m => m.Name == nameof(EntityTypeBuilder.Property)
                && m.IsGenericMethod
                && m.GetParameters().Length == 1
                && m.GetParameters()[0].ParameterType.IsGenericType
                && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>))
            .MakeGenericMethod(propertyLambdaType.Type)
            .Invoke(entity, [propertyLambda]) as PropertyBuilder ??
            throw new InvalidOperationException($"Could not create PropertyBuilder for property '{propertyName}' on entity type '{entityType.Name}'");

        return propertyBuilder;
    }

    public static ReferenceNavigationBuilder HasOneGeneric(this EntityTypeBuilder entity, Type masterEntityType, string detailPropertyName)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(masterEntityType, nameof(masterEntityType));
        ArgumentException.ThrowIfNullOrWhiteSpace(detailPropertyName, nameof(detailPropertyName));

        var entityType = entity.Metadata.ClrType;
        var entityTypeBuilderGenericType = typeof(EntityTypeBuilder<>).MakeGenericType(entityType);

        var (hasOneProperty, hasOneLambda) = entityType.LambdaExpressionGenericEntityProperty(detailPropertyName);

        var referenceNavigationBuilder = entityTypeBuilderGenericType
                .GetMethods()
                .First(m => m.Name == nameof(EntityTypeBuilder.HasOne)
                    && m.IsGenericMethod
                    && m.GetParameters().Length == 1
                    && m.GetParameters()[0].ParameterType.IsGenericType
                    && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>))
                .MakeGenericMethod(masterEntityType)
                .Invoke(entity, [hasOneLambda]) as ReferenceNavigationBuilder ??
                throw new InvalidOperationException($"Could not create ReferenceNavigationBuilder for property '{detailPropertyName}' on entity type '{entityType.Name}'");

        return referenceNavigationBuilder;
    }

    public static ReferenceCollectionBuilder WithManyGeneric(this ReferenceNavigationBuilder navigation, Type masterEntityType, Type detailEntityType, string masterPropertyName)
    {
        ArgumentNullException.ThrowIfNull(navigation, nameof(navigation));
        ArgumentNullException.ThrowIfNull(masterEntityType, nameof(masterEntityType));
        ArgumentNullException.ThrowIfNull(detailEntityType, nameof(detailEntityType));
        ArgumentNullException.ThrowIfNull(masterPropertyName, nameof(masterPropertyName));

        var (_, withManyLambda) = masterEntityType.LambdaExpressionGenericEntityPropertyAsDelegate(masterPropertyName,
            typeof(Func<,>).MakeGenericType(masterEntityType, typeof(IEnumerable<>).MakeGenericType(detailEntityType)));

        var referenceCollectionBuilder = navigation.GetType()
            .GetMethods()
            .First(m => m.Name == nameof(ReferenceNavigationBuilder.WithMany)
                && !m.IsGenericMethod
                && m.GetParameters().Length == 1
                && m.GetParameters()[0].ParameterType.IsGenericType
                && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>))
            .Invoke(navigation, [withManyLambda]) as ReferenceCollectionBuilder ??
            throw new InvalidOperationException($"Could not create ReferenceCollectionBuilder for property '{masterPropertyName}' on entity type '{masterEntityType.Name}'");

        return referenceCollectionBuilder;
    }

    public static EntityTypeBuilder HasQueryFilterGeneric(this EntityTypeBuilder entity, LambdaExpression lambdaExpression, string? filterName = null)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(lambdaExpression, nameof(lambdaExpression));

        var entityType = entity.Metadata.ClrType;
        var entityTypeBuilderGenericType = typeof(EntityTypeBuilder<>).MakeGenericType(entityType);

        if (string.IsNullOrWhiteSpace(filterName))
        {
            _ = entityTypeBuilderGenericType
                .GetMethods()
                .First(m => m.Name == nameof(EntityTypeBuilder.HasQueryFilter)
                    && !m.IsGenericMethod
                    && m.GetParameters().Length == 1
                    && m.GetParameters()[0].ParameterType.IsGenericType
                    && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>))
                .Invoke(entity, [lambdaExpression]);
        }
        else
        {
            _ = entityTypeBuilderGenericType
                .GetMethods()
                .First(m => m.Name == nameof(EntityTypeBuilder<>.HasQueryFilter)
                    && m.GetParameters().Length == 2
                    && m.GetParameters()[0].ParameterType == typeof(string)
                    && m.GetParameters()[1].ParameterType.IsGenericType
                    && m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>))
                .Invoke(entity, [filterName, lambdaExpression]);
        }

        return entity;
    }

    public static (MemberExpression, LambdaExpression) LambdaExpressionGenericEntityProperty(this Type entityType, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(entityType, nameof(entityType));
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));

        var lambdaParameter = Expression.Parameter(entityType, "entity");
        var lambdaProperty = Expression.Property(lambdaParameter, propertyName);
        var lambdaExpression = Expression.Lambda(lambdaProperty, lambdaParameter);

        return (lambdaProperty, lambdaExpression);
    }

    public static (MemberExpression, LambdaExpression) LambdaExpressionGenericEntityPropertyAsDelegate(this Type entityType, string propertyName, Type delegateType)
    {
        ArgumentNullException.ThrowIfNull(entityType, nameof(entityType));
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));

        var lambdaParameter = Expression.Parameter(entityType, "entity");
        var lambdaProperty = Expression.Property(lambdaParameter, propertyName);
        var lambdaExpression = Expression.Lambda(delegateType, lambdaProperty, lambdaParameter);

        return (lambdaProperty, lambdaExpression);
    }

    public static LambdaExpression LambdaExpressionGenericEntityPropertyEquals(this Type entityType, string propertyName, object? value)
    {
        ArgumentNullException.ThrowIfNull(entityType, nameof(entityType));
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));

        var lambdaParameter = Expression.Parameter(entityType, "entity");
        var lampdaProperty = Expression.Property(lambdaParameter, propertyName);
        var lambdaValue = Expression.Constant(value);
        var lambdaOperation = Expression.Equal(lampdaProperty, lambdaValue);
        var lambdaDelegateType = typeof(Func<,>).MakeGenericType(entityType, typeof(bool));
        var lambdaExpression = Expression.Lambda(lambdaDelegateType, lambdaOperation, lambdaParameter);

        return lambdaExpression;
    }
}
