using FFCEI.DotNetStack.Models.Attributes;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection;

namespace FFCEI.DotNetStack.Models.Reflection;

[DebuggerDisplay("EntityModelPropertyMap: {EntityModelType?.Name,nq} [Properties: {Properties?.Count}, Foreign Keys: {ForeignKeyReferences?.Count} Unique Keys: {UniqueKeys?.Count} Navigation: {ForeignKeyNavigationReferences.Count}]")]
public sealed class EntityModelPropertyMap
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal ConcurrentDictionary<Type, ForeignKeyReferenceNavigationMetadata> InternalForeignKeyNavigationReferences = new();

    public Type EntityModelType { get; private set; }
    public IReadOnlyCollection<ModelPropertyMetadata> Properties { get; private set; } = [];
    public IReadOnlyCollection<ForeignKeyReferenceMetadata> ForeignKeyReferences { get; private set; } = [];
    public IReadOnlyCollection<UniqueKeyReferenceMetadata> UniqueKeys { get; private set; } = [];
    public IReadOnlyCollection<ForeignKeyReferenceNavigationMetadata> ForeignKeyNavigationReferences => InternalForeignKeyNavigationReferences.Values.ToList().AsReadOnly();

    internal EntityModelPropertyMap(Type entityModelType)
    {
        ArgumentNullException.ThrowIfNull(entityModelType, nameof(entityModelType));

        EntityModelType = entityModelType;

        var implementationTypes = entityModelType.EntityModelImplementationTypes();

        if (implementationTypes.Length == 0)
        {
            throw new InvalidOperationException($"Type {entityModelType.Name} does not implement any IEntityModel interface");
        }

        var implementedProperties = implementationTypes.SelectMany(modelType => modelType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(property => (property.DeclaringType == null || !property.DeclaringType.IsInterface) && !property.Name.Contains('.', StringComparison.InvariantCulture))
            .GroupBy(property => property.Name)
            .Select(grouped => grouped.First())
            .OrderBy(property => property.Name)
            .ToList();

        var modelProperties = implementedProperties.Where(property => property.CanWrite || property.IsEntityModelNavigationCollection()).ToList();
        var modelForeignKeys = entityModelType.GetCustomAttributes<ForeignKeyReferenceAttribute>().ToList();

        Properties = modelProperties.Select(property => new ModelPropertyMetadata(property)).ToList().AsReadOnly();

        var propertyForeignKeys = implementedProperties.SelectMany(property => property.GetCustomAttributes<ForeignKeyAttribute>()).ToList();
        var entityForeignKeyReferences = modelForeignKeys.Select(attribute => new ForeignKeyReferenceMetadata(attribute, Properties)).ToList();
        var propertyForeignKeyReferences = propertyForeignKeys.Select(attribute => new ForeignKeyReferenceMetadata(attribute, Properties)).ToList();

        ForeignKeyReferences = entityForeignKeyReferences.Concat(propertyForeignKeyReferences).ToList().AsReadOnly();

        var modelUniqueKeys = entityModelType.GetCustomAttributes<IndexAttribute>().Where(index => index.IsUnique).ToList();

        UniqueKeys = modelUniqueKeys.Select(attribute => new UniqueKeyReferenceMetadata(attribute, Properties)).ToList().AsReadOnly();
    }
}
