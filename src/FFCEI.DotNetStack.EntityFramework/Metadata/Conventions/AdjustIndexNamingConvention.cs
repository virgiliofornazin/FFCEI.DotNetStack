using FFCEI.DotNetStack.EntityFramework.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.EntityFramework.Metadata.Conventions;

internal sealed class AdjustIndexNamingConvention(ILogger logger)
    : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        var entityTypes = modelBuilder.Metadata.GetEntityTypes().ToList();

        foreach (var entityType in entityTypes)
        {
            var entityTypeModelImplementationType = entityType.ClrType;

            if (entityTypeModelImplementationType == null)
            {
                continue;
            }

            var entityBuilder = modelBuilder.Entity(entityTypeModelImplementationType);
            var entityName = entityType.ShortName();
            var indexes = entityType.GetIndexes().ToList();

            foreach (var index in indexes)
            {
                var indexName = index.GetDatabaseName();
                var indexSource = index.GetConfigurationSource();

                if (indexSource == ConfigurationSource.Convention)
                {
                    var foreignKeysMatchingIndex = entityType.GetForeignKeys().FirstOrDefault(fk => fk.Properties.SequenceEqual(index.Properties));
                    var foreignKeyConstraintName = foreignKeysMatchingIndex?.GetConstraintName();

                    if (!string.IsNullOrWhiteSpace(foreignKeyConstraintName))
                    {
                        _ = index.SetDatabaseName(foreignKeyConstraintName);

                        continue;
                    }
                }

                var shouldGenerateName = string.IsNullOrWhiteSpace(indexName) || ((indexSource == ConfigurationSource.DataAnnotation) && indexName.StartsWith("IX_", StringComparison.InvariantCulture));

                if (shouldGenerateName && !string.IsNullOrWhiteSpace(indexName))
                {
                    var indexNameSplitted = indexName.Split("_");
                    var indexEntityName = indexNameSplitted.Length > 1 ? indexNameSplitted[1] : null;
                    var indexColumnNames = indexNameSplitted.Length > 2 ? indexNameSplitted.Skip(2).ToHashSet() : [];

                    shouldGenerateName = (indexEntityName == entityName) && (index.Properties.Count(property => indexColumnNames.Contains(property.Name)) == index.Properties.Count);
                }

                if (shouldGenerateName)
                {
                    if (entityBuilder != null)
                    {
                        var entityCounters = new EntityModelDbContextModelBuilderObjectNameGenerator(entityBuilder);
                        var previousIndexName = indexName;

                        indexName = index.IsUnique ? entityCounters.GenerateUniqueKeyName() : entityCounters.GenerateIndexName();

                        _ = index.SetDatabaseName(indexName);

                        EntityFrameworkLog.RenamedIndex(logger, previousIndexName ?? string.Empty, indexName, entityName);

                        continue;
                    }
                }
            }
        }
    }
}

