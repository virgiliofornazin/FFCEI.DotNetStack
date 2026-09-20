using FFCEI.DotNetStack.EntityFramework.Attributes;
using FFCEI.DotNetStack.EntityFramework.Reflection;
using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes.Rules;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Models;
using FFCEI.DotNetStack.Models.Interfaces;
using FFCEI.DotNetStack.Models.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Reflection;

namespace FFCEI.DotNetStack.EntityFramework.Metadata.Builders;

public static class EntityModelDbContextModelBuilder
{
    private static readonly string[] _skippedAssemblyNames = ["FFCEI.DotNetStack.EntityFramework", "FFCEI.DotNetStack.Models"];

    public static void BuildModels(ModelBuilder modelBuilder, DatabaseProviderType databaseProvider, Assembly startingAssembly, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));
        ArgumentNullException.ThrowIfNull(startingAssembly, nameof(startingAssembly));

        logger ??= NullLogger.Instance;

        _ = modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);

        if (databaseProvider == DatabaseProviderType.Postgres)
        {
            _ = modelBuilder.HasCollation(
                name: MappingRules.PostgresUnicodeCollation,
                locale: MappingRules.PostgresUnicodeLocale,
                provider: "icu",
                deterministic: false);
        }

        var startingAssemblyFullName = startingAssembly.FullName ?? string.Empty;

        if (string.IsNullOrWhiteSpace(startingAssemblyFullName))
        {
            return;
        }

        var referencedAssemblies = startingAssembly.GetReferencedAssemblies();

        if (referencedAssemblies == null)
        {
            return;
        }

        var assembliesMap = new ConcurrentDictionary<string, Assembly>();

        assembliesMap[startingAssemblyFullName] = startingAssembly;

        if (!_skippedAssemblyNames.Contains(startingAssembly.GetName().Name))
        {
            foreach (var referencedAssembly in referencedAssemblies)
            {
                var assemblyName = referencedAssembly.Name ?? string.Empty;

                if (!_skippedAssemblyNames.Contains(assemblyName))
                {
                    if (assemblyName.EndsWith(".Models", StringComparison.InvariantCulture))
                    {
                        _ = assembliesMap.TryGetOrAddReference(assemblyName, (_) => { return Assembly.Load(assemblyName); });

                        var assemblyEntityFrameworkName = assemblyName.Replace(".Models", ".EntityFramework", StringComparison.InvariantCulture);

                        _ = assembliesMap.TryGetOrAddReference(assemblyEntityFrameworkName, (_) => { return Assembly.Load(assemblyEntityFrameworkName); });
                    }
                }
            }
        }

        var assemblies = assembliesMap.Values.Distinct().ToList();

        _ = Parallel.ForEach(assemblies, (assembly) =>
        {
            _ = EntityModelDbContextModelBuilderAssemblyCache.Instance.GetOrAdd(assembly);
        });

        var mappedAssemblies = EntityModelDbContextModelBuilderAssemblyCache.Instance.Values().Where(assembly => assemblies.Contains(assembly.EntityModelAssembly)).ToList();

        foreach (var mappedAssembly in mappedAssemblies.Where(mappedAssembly => mappedAssembly.EntityTypeModelImplementations.Count != 0))
        {
            foreach (var entityTypeModelImplementationType in mappedAssembly.EntityTypeModelImplementations)
            {
                var entityModelNamespace = entityTypeModelImplementationType.Namespace;

                if (string.IsNullOrWhiteSpace(entityModelNamespace) || !mappedAssembly.EntityModelNamespaceDatabaseSchemaMap.TryGetValue(entityModelNamespace, out var databaseSchema))
                {
                    databaseSchema = null;
                }

                MapEntityPropertiesToModelBuilder(modelBuilder, entityTypeModelImplementationType, databaseProvider, databaseSchema, logger);
            }
        }

        foreach (var mappedAssembly in mappedAssemblies.Where(mappedAssembly => mappedAssembly.EntityTypeModelImplementations.Count != 0))
        {
            foreach (var entityTypeModelImplementationType in mappedAssembly.EntityTypeModelImplementations)
            {
                MapForeignKeysToModelBuilder(modelBuilder, entityTypeModelImplementationType);
            }
        }

        foreach (var mappedAssembly in mappedAssemblies.Where(mappedAssembly => mappedAssembly.EntityTypeConfigurationImplementations.Count != 0))
        {
            _ = modelBuilder.ApplyConfigurationsFromAssembly(mappedAssembly.EntityModelAssembly);
        }
    }

    private static void MapEntityPropertiesToModelBuilder(ModelBuilder modelBuilder, Type entityTypeModelImplementationType, DatabaseProviderType databaseProvider, string? databaseSchema, ILogger logger)
    {
        if (entityTypeModelImplementationType.GetCustomAttributeFromEntityModelImplementationsOrInterface<IgnoreEntityModelAttribute>() != null)
        {
            return;
        }

        var entity = modelBuilder.EntityGeneric(entityTypeModelImplementationType);

        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var entityCounters = new EntityModelDbContextModelBuilderObjectNameGenerator(entity);
        var objectName = entityTypeModelImplementationType.Name;
        var hasKey = false;
        var isView = entityTypeModelImplementationType.Implements<IViewEntityModel>();

        if (isView)
        {
            _ = entity.ToView(objectName, databaseSchema);

            EntityFrameworkLog.MappedView(logger, entityTypeModelImplementationType.Name, objectName);
        }
        else
        {
            _ = entity.ToTable(objectName, databaseSchema);

            EntityFrameworkLog.MappedTable(logger, entityTypeModelImplementationType.Name, objectName);
        }

        if (entityTypeModelImplementationType.Implements<IDatabaseIdEntityModel>())
        {
            var idProperty = entity.Property(nameof(IDatabaseIdEntityModel.Id));

            if (!isView)
            {
                _ = idProperty
                    .IsRequired(true)
                    .ValueGeneratedOnAdd();

                _ = entity.HasKey(nameof(IDatabaseIdEntityModel.Id))
                    .HasName($"{objectName}_PK");

                hasKey = true;
            }
        }

        if (entityTypeModelImplementationType.Implements<IUniversalUniqueIdEntityModel>())
        {
            var universalUniqueIdProperty = entity.Property(nameof(IUniversalUniqueIdEntityModel.UniversalUniqueId));

            if (!isView)
            {
                _ = universalUniqueIdProperty
                    .IsRequired(true);

                if (!hasKey)
                {
                    _ = entity.HasKey(nameof(IUniversalUniqueIdEntityModel.UniversalUniqueId))
                        .HasName($"{objectName}_PK");

                    hasKey = true;
                }
                else
                {
                    _ = entity.HasIndex(nameof(IUniversalUniqueIdEntityModel.UniversalUniqueId))
                        .IsUnique(true)
                        .HasDatabaseName(entityCounters.GenerateUniqueKeyName());
                }
            }
        }

        if (!hasKey)
        {
            _ = entity.HasNoKey();
        }

        if (entityTypeModelImplementationType.Implements<ILogicallyDeletableEntityModel>())
        {
            var isLogicallyDeletedProperty = entity.Property(nameof(ILogicallyDeletableEntityModel.IsLogicallyDeleted));

            var logicallyDeletedTimestampProperty = entity.Property(nameof(ILogicallyDeletableEntityModel.LogicallyDeletedTimestamp));

            var wasLogicallyDeletedCascadedProperty = entity.Property(nameof(ILogicallyDeletableEntityModel.WasLogicallyDeletedCascaded));

            if (!isView)
            {
                _ = isLogicallyDeletedProperty
                    .IsRequired(true);

                _ = logicallyDeletedTimestampProperty
                    .IsRequired(false);

                _ = wasLogicallyDeletedCascadedProperty
                    .IsRequired(true);

                var isLogicallyDeletedQueryFilterLambda = entityTypeModelImplementationType.LambdaExpressionGenericEntityPropertyEquals(
                    nameof(ILogicallyDeletableEntityModel.IsLogicallyDeleted), false);

                _ = entity.HasQueryFilterGeneric(isLogicallyDeletedQueryFilterLambda, QueryFilters.IsLogicallyDeletedQueryFilterName);
            }
        }

        if (entityTypeModelImplementationType.Implements<ITimestampTrackedEntityModel>())
        {
            var createdTimestampProperty = entity.Property(nameof(ITimestampTrackedEntityModel.CreatedTimestamp));

            var updatedTimestampProperty = entity.Property(nameof(ITimestampTrackedEntityModel.UpdatedTimestamp));

            if (!isView)
            {
                _ = createdTimestampProperty
                    .IsRequired(true);

                _ = updatedTimestampProperty
                    .IsRequired(true);
            }
        }

        var properties = entity.Metadata.GetProperties().ToList();

        foreach (var property in properties)
        {
            if (property.ClrType.IsInterface && property.ClrType.IsEntityModelDerivedType())
            {
                _ = entity.Ignore(property.Name);

                continue;
            }

            var entityProperty = entity.Property(property.Name);

            ArgumentNullException.ThrowIfNull(entityProperty, nameof(entityProperty));

            _ = entityProperty.ApplyDefaultColumnProperties(isView, databaseProvider);
        }
    }

    private static void MapForeignKeysToModelBuilder(ModelBuilder modelBuilder, Type entityTypeModelImplementationType)
    {
        if (entityTypeModelImplementationType.GetCustomAttributeFromEntityModelImplementationsOrInterface<IgnoreEntityModelAttribute>() != null)
        {
            return;
        }

        var entity = modelBuilder.EntityGeneric(entityTypeModelImplementationType);
        var entityCounters = new EntityModelDbContextModelBuilderObjectNameGenerator(entity);
        var entityModelPropertyMap = EntityModelPropertyMapCache.Instance.GetOrAdd(entityTypeModelImplementationType);
        var foreignKeyReferences = entityModelPropertyMap.ForeignKeyReferences;

        foreach (var foreignKeyReference in foreignKeyReferences)
        {
            MapForeignKeyToModelBuilder(entityTypeModelImplementationType, entity, entityCounters, entityModelPropertyMap, foreignKeyReference);
        }
    }

    private static void MapForeignKeyToModelBuilder(Type entityTypeModelImplementationType, EntityTypeBuilder entity, EntityModelDbContextModelBuilderObjectNameGenerator entityCounters, EntityModelPropertyMap entityModelPropertyMap, ForeignKeyReferenceMetadata foreignKeyReference)
    {
        var isRequired = foreignKeyReference.IsRequired;
        var deleteBehaviour = foreignKeyReference.DeleteBehavior;
        var constraintName = foreignKeyReference.ConstraintName ?? entityCounters.GenerateForeignKeyName();

        ArgumentNullException.ThrowIfNull(entityModelPropertyMap, nameof(entityModelPropertyMap));

        var foreignKeyEntityType = foreignKeyReference.EntityProperty.PropertyType;
        var reverseNavigationPropertyName = foreignKeyReference.ReverseNavigationPropertyName;
        var foreignKeyEntityProperty = foreignKeyReference.EntityProperty;
        var foreignKeyEntityPropertyName = foreignKeyEntityProperty.Name;
        var foreignKeyIdPropertyName = foreignKeyReference.IdPropertyName ?? string.Empty;
        var foreignKeyIdProperty = entityModelPropertyMap.Properties.FirstOrDefault(p => p.Name == foreignKeyIdPropertyName);
        var entityModelIdPropertyName = $"{foreignKeyEntityPropertyName}{nameof(IDatabaseIdEntityModel.Id)}";
        var entityModelIdProperty = entityModelPropertyMap.Properties.FirstOrDefault(p => p.Name == entityModelIdPropertyName);
        var entityModelUniversalUniqueIdPropertyName = $"{foreignKeyEntityPropertyName}{nameof(IUniversalUniqueIdEntityModel.UniversalUniqueId)}";
        var entityModelUniversalUniqueIdProperty = entityModelPropertyMap.Properties.FirstOrDefault(p => p.Name == entityModelUniversalUniqueIdPropertyName);

        foreignKeyIdPropertyName = !string.IsNullOrWhiteSpace(foreignKeyIdPropertyName) ?
            foreignKeyIdPropertyName : (foreignKeyIdProperty == entityModelIdProperty ? entityModelIdPropertyName :
            (foreignKeyIdProperty == entityModelUniversalUniqueIdProperty ? entityModelUniversalUniqueIdPropertyName : null));

        foreignKeyIdProperty ??= foreignKeyIdPropertyName == entityModelIdPropertyName ? entityModelIdProperty :
            (foreignKeyIdPropertyName == entityModelUniversalUniqueIdPropertyName ? entityModelUniversalUniqueIdProperty : null);

        if (foreignKeyIdProperty != null && foreignKeyIdPropertyName != null)
        {
            var propertyBuilder = entity
                .PropertyGeneric(foreignKeyIdPropertyName);

            _ = propertyBuilder
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            if (isRequired.HasValue)
            {
                _ = propertyBuilder
                    .IsRequired(isRequired.Value);
            }

            var referenceNavigationBuilder = entity
                .HasOneGeneric(foreignKeyEntityType, foreignKeyEntityPropertyName);

            IConventionForeignKeyBuilder? conventionForeignKeyBuilder = null;

            if (string.IsNullOrWhiteSpace(reverseNavigationPropertyName))
            {
                conventionForeignKeyBuilder = referenceNavigationBuilder.WithMany() as IConventionForeignKeyBuilder;
            }
            else
            {
                var foreignEntityPropertyMap = EntityModelPropertyMapCache.Instance.GetOrAdd(foreignKeyEntityType);

                ArgumentNullException.ThrowIfNull(foreignEntityPropertyMap, nameof(foreignEntityPropertyMap));

                var property = foreignEntityPropertyMap.Properties.FirstOrDefault(p => p.Name == reverseNavigationPropertyName);

                ArgumentNullException.ThrowIfNull(property, nameof(property));

                if (property.PropertyInfo.PropertyType.IsEntityModelNavigationCollection())
                {
                    conventionForeignKeyBuilder = (referenceNavigationBuilder
                        .WithManyGeneric(foreignKeyEntityType, entityTypeModelImplementationType, reverseNavigationPropertyName)
                        .HasForeignKey(foreignKeyIdPropertyName)
                        .HasConstraintName(constraintName) as IInfrastructure<IConventionForeignKeyBuilder>)?.Instance;
                }
                else
                {
                    conventionForeignKeyBuilder = (referenceNavigationBuilder
                        .WithOneGeneric(foreignKeyEntityType, entityTypeModelImplementationType, reverseNavigationPropertyName)
                        .HasForeignKey(entityTypeModelImplementationType, foreignKeyIdPropertyName)
                        .HasConstraintName(constraintName) as IInfrastructure<IConventionForeignKeyBuilder>)?.Instance;
                }

                _ = foreignEntityPropertyMap.InternalForeignKeyNavigationReferences.GetOrAdd(entityTypeModelImplementationType,
                    (_) => new ForeignKeyReferenceNavigationMetadata(entityTypeModelImplementationType, foreignKeyReference));
            }

            if (isRequired.HasValue)
            {
                _ = conventionForeignKeyBuilder?
                    .IsRequired(isRequired.Value);
            }

            if (deleteBehaviour.HasValue)
            {
                _ = conventionForeignKeyBuilder?
                    .OnDelete(deleteBehaviour.Value);
            }
        }

        if (entityModelIdProperty != foreignKeyIdProperty)
        {
            _ = entity.Ignore(entityModelIdPropertyName);
        }

        if (entityModelUniversalUniqueIdProperty != foreignKeyIdProperty)
        {
            _ = entity.Ignore(entityModelUniversalUniqueIdPropertyName);
        }

        if (foreignKeyIdProperty != null)
        {
            _ = entity
                .Navigation(foreignKeyEntityPropertyName)
                .UsePropertyAccessMode(PropertyAccessMode.Property);
        }
    }
}
