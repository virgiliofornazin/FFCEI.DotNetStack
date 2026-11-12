using FFCEI.DotNetStack.EntityModels.Attributes;
using FFCEI.DotNetStack.EntityModels.Interfaces;
using FFCEI.DotNetStack.Framework;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection;

namespace FFCEI.DotNetStack.EntityModels.Reflection;

[DebuggerDisplay("ForeignKeyReference: {EntityPropertyName,nq} (EntityModelType: {EntityProperty.PropertyType.Name,nq}) {ReverseNavigationPropertyName,nq}")]
public sealed class ForeignKeyReferenceMetadata
{
    internal ForeignKeyReferenceMetadata(ForeignKeyReferenceAttribute foreignKeyReferenceAttribute, IReadOnlyCollection<EntityModelPropertyMetadata> properties)
    {
        ArgumentNullException.ThrowIfNull(foreignKeyReferenceAttribute, nameof(foreignKeyReferenceAttribute));
        ArgumentNullException.ThrowIfNull(properties, nameof(properties));

        EntityPropertyName = foreignKeyReferenceAttribute.EntityPropertyName;
        IdPropertyName = foreignKeyReferenceAttribute.IdPropertyNameHasValue ? foreignKeyReferenceAttribute.IdPropertyName : null;
        ReverseNavigationPropertyName = foreignKeyReferenceAttribute.ReverseNavigationPropertyNameHasValue ? foreignKeyReferenceAttribute.ReverseNavigationPropertyName : null;
        ConstraintName = foreignKeyReferenceAttribute.ConstraintNameHasValue ? foreignKeyReferenceAttribute.ConstraintName : null;
        IsRequired = foreignKeyReferenceAttribute.IsRequiredHasValue ? foreignKeyReferenceAttribute.IsRequired : null;
        DeleteBehavior = foreignKeyReferenceAttribute.DeleteBehaviorHasValue ? foreignKeyReferenceAttribute.DeleteBehavior : null;

        EntityProperty = RequirePropertyInfo(properties, EntityPropertyName, "ForeignKeyReferenceAttribute:EntityPropertyName");
        IdProperty = string.IsNullOrWhiteSpace(IdPropertyName) ? null! : RequirePropertyInfo(properties, IdPropertyName, "ForeignKeyReferenceAttribute:IdPropertyName");

        LookupMissingIdProperties(properties);
        FixUpIdProperty(properties, "ForeignKeyReferenceAttribute");
        FixupRequiredAndDeleteBehaviour();
    }

    internal ForeignKeyReferenceMetadata(ForeignKeyAttribute foreignKeyAttribute, IReadOnlyCollection<EntityModelPropertyMetadata> properties)
    {
        ArgumentNullException.ThrowIfNull(foreignKeyAttribute, nameof(foreignKeyAttribute));
        ArgumentNullException.ThrowIfNull(properties, nameof(properties));

        var referencedProperty = RequirePropertyInfo(properties, foreignKeyAttribute.Name, "ForeignKeyAttribute:Name");

        if (referencedProperty.PropertyType.IsEntityModelImplementationType())
        {
            EntityProperty = referencedProperty;
            EntityPropertyName = EntityProperty.Name;

            IdPropertyName = null;
            IdProperty = null!;
        }
        else if (referencedProperty.Name.EndsWith(nameof(IUniversalUniqueIdEntityModel.UniversalUniqueId), StringComparison.InvariantCulture))
        {
            if (referencedProperty.PropertyType.NonNullableType() != typeof(Guid))
            {
                throw new InvalidOperationException($"Property '{referencedProperty.Name}' ends with '{nameof(IUniversalUniqueIdEntityModel.UniversalUniqueId)}' but its type is {referencedProperty.PropertyType.CSharpTypeName()}, not Guid.");
            }

            UniversalUniqueIdProperty = referencedProperty;

            IdPropertyName = UniversalUniqueIdProperty.Name;
            IdProperty = UniversalUniqueIdProperty;

            EntityPropertyName = referencedProperty.Name[..^nameof(IUniversalUniqueIdEntityModel.UniversalUniqueId).Length];
            EntityProperty = RequirePropertyInfo(properties, EntityPropertyName, "ForeignKeyAttribute:EntityPropertyName(UniversalUniqueId)");

            IsRequired = !UniversalUniqueIdProperty.PropertyType.IsNullable();
        }
        else if (referencedProperty.Name.EndsWith(nameof(IDatabaseUniqueIdEntityModel.Id), StringComparison.InvariantCulture))
        {
            if (referencedProperty.PropertyType.NonNullableType() != typeof(long))
            {
                throw new InvalidOperationException($"Property '{referencedProperty.Name}' ends with '{nameof(IDatabaseUniqueIdEntityModel.Id)}' but its type is {referencedProperty.PropertyType.CSharpTypeName()}, not long.");
            }

            DatabaseUniqueIdProperty = referencedProperty;

            IdPropertyName = DatabaseUniqueIdProperty.Name;
            IdProperty = DatabaseUniqueIdProperty;

            EntityPropertyName = referencedProperty.Name[..^nameof(IDatabaseUniqueIdEntityModel.Id).Length];
            EntityProperty = RequirePropertyInfo(properties, EntityPropertyName, "ForeignKeyAttribute:EntityPropertyName(DatabaseUniqueId)");

            IsRequired = !DatabaseUniqueIdProperty.PropertyType.IsNullable();
        }
        else
        {
            throw new InvalidOperationException($"Cannot resolve foreign key reference");
        }

        LookupMissingIdProperties(properties);
        FixUpIdProperty(properties, "ForeignKeyAttribute");
        FixupRequiredAndDeleteBehaviour();
    }

    private static PropertyInfo RequirePropertyInfo(IReadOnlyCollection<EntityModelPropertyMetadata> properties, string propertyName, string attributeContext)
    {
        var property = properties.FirstOrDefault(p => p.Name == propertyName);

        return property is null
            ? throw new InvalidOperationException($"{attributeContext}: property '{propertyName}' was not found among the entity writable properties.")
            : property.PropertyInfo;
    }

    private void FixUpIdProperty(IReadOnlyCollection<EntityModelPropertyMetadata> properties, string scopeName)
    {
        if (string.IsNullOrWhiteSpace(IdPropertyName))
        {
            var propertyCascadeChoice = (UniversalUniqueIdProperty ?? DatabaseUniqueIdProperty ?? EntityProperty);

            IdPropertyName = propertyCascadeChoice?.Name;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(IdPropertyName, nameof(IdPropertyName));

        IdProperty ??= RequirePropertyInfo(properties, IdPropertyName, $"{scopeName}:IdPropertyName");
    }

    private void LookupMissingIdProperties(IReadOnlyCollection<EntityModelPropertyMetadata> properties)
    {
        DatabaseUniqueIdProperty ??= properties.FirstOrDefault(property => property.Name == $"{EntityPropertyName}{nameof(IDatabaseUniqueIdEntityModel.Id)}")?.PropertyInfo;
        UniversalUniqueIdProperty ??= properties.FirstOrDefault(property => property.Name == $"{EntityPropertyName}{nameof(IUniversalUniqueIdEntityModel.UniversalUniqueId)}")?.PropertyInfo;
    }

    private void FixupRequiredAndDeleteBehaviour()
    {
        IsRequired ??= !(UniversalUniqueIdProperty ?? DatabaseUniqueIdProperty ?? EntityProperty)?.PropertyType.IsNullable() ?? false;
        DeleteBehavior ??= (IsRequired ?? false) ? Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict : Microsoft.EntityFrameworkCore.DeleteBehavior.NoAction;
    }

    public string EntityPropertyName { get; }
    public string? IdPropertyName { get; set; }
    public string? ReverseNavigationPropertyName { get; set; }
    public string? ConstraintName { get; private set; }
    public bool? IsRequired { get; private set; }
    public DeleteBehavior? DeleteBehavior { get; private set; }

    public PropertyInfo EntityProperty { get; set; }
    public PropertyInfo IdProperty { get; set; }
    public PropertyInfo? DatabaseUniqueIdProperty { get; set; }
    public PropertyInfo? UniversalUniqueIdProperty { get; set; }
}
