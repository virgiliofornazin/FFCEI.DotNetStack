using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Models.Attributes;
using FFCEI.DotNetStack.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection;

namespace FFCEI.DotNetStack.Models.Reflection;

[DebuggerDisplay("ForeignKeyReference: {EntityPropertyName,nq} (EntityModelType: {EntityProperty.PropertyType.Name,nq}) {ReverseNavigationPropertyName,nq}")]
public sealed class ForeignKeyReferenceMetadata
{
    internal ForeignKeyReferenceMetadata(ForeignKeyReferenceAttribute foreignKeyReferenceAttribute, IReadOnlyCollection<ModelPropertyMetadata> properties)
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

    internal ForeignKeyReferenceMetadata(ForeignKeyAttribute foreignKeyAttribute, IReadOnlyCollection<ModelPropertyMetadata> properties)
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
        else if (referencedProperty.Name.EndsWith(nameof(IExternalUniqueIdEntityModel.ExternalUniqueId), StringComparison.InvariantCulture))
        {
            if (referencedProperty.PropertyType.NonNullableType() != typeof(Guid))
            {
                throw new InvalidOperationException($"Property '{referencedProperty.Name}' ends with '{nameof(IExternalUniqueIdEntityModel.ExternalUniqueId)}' but its type is {referencedProperty.PropertyType.CSharpTypeName()}, not Guid.");
            }

            ExternalUniqueIdProperty = referencedProperty;

            IdPropertyName = ExternalUniqueIdProperty.Name;
            IdProperty = ExternalUniqueIdProperty;

            EntityPropertyName = referencedProperty.Name[..^nameof(IExternalUniqueIdEntityModel.ExternalUniqueId).Length];
            EntityProperty = RequirePropertyInfo(properties, EntityPropertyName, "ForeignKeyAttribute:EntityPropertyName(ExternalUniqueId)");

            IsRequired = !ExternalUniqueIdProperty.PropertyType.IsNullable();
        }
        else if (referencedProperty.Name.EndsWith(nameof(IUniqueIdEntityModel.Id), StringComparison.InvariantCulture))
        {
            if (referencedProperty.PropertyType.NonNullableType() != typeof(long))
            {
                throw new InvalidOperationException($"Property '{referencedProperty.Name}' ends with '{nameof(IUniqueIdEntityModel.Id)}' but its type is {referencedProperty.PropertyType.CSharpTypeName()}, not long.");
            }

            UniqueIdProperty = referencedProperty;

            IdPropertyName = UniqueIdProperty.Name;
            IdProperty = UniqueIdProperty;

            EntityPropertyName = referencedProperty.Name[..^nameof(IUniqueIdEntityModel.Id).Length];
            EntityProperty = RequirePropertyInfo(properties, EntityPropertyName, "ForeignKeyAttribute:EntityPropertyName(UniqueId)");

            IsRequired = !UniqueIdProperty.PropertyType.IsNullable();
        }
        else
        {
            throw new InvalidOperationException($"Cannot resolve foreign key reference");
        }

        LookupMissingIdProperties(properties);
        FixUpIdProperty(properties, "ForeignKeyAttribute");
        FixupRequiredAndDeleteBehaviour();
    }

    private static PropertyInfo RequirePropertyInfo(IReadOnlyCollection<ModelPropertyMetadata> properties, string propertyName, string attributeContext)
    {
        var property = properties.FirstOrDefault(p => p.Name == propertyName);

        return property is null
            ? throw new InvalidOperationException($"{attributeContext}: property '{propertyName}' was not found among the model's writable properties.")
            : property.PropertyInfo;
    }

    private void FixUpIdProperty(IReadOnlyCollection<ModelPropertyMetadata> properties, string scopeName)
    {
        if (string.IsNullOrWhiteSpace(IdPropertyName))
        {
            var propertyCascadeChoice = (ExternalUniqueIdProperty ?? UniqueIdProperty ?? EntityProperty);

            IdPropertyName = propertyCascadeChoice?.Name;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(IdPropertyName, nameof(IdPropertyName));

        IdProperty ??= RequirePropertyInfo(properties, IdPropertyName, $"{scopeName}:IdPropertyName");
    }

    private void LookupMissingIdProperties(IReadOnlyCollection<ModelPropertyMetadata> properties)
    {
        UniqueIdProperty ??= properties.FirstOrDefault(property => property.Name == $"{EntityPropertyName}{nameof(IUniqueIdEntityModel.Id)}")?.PropertyInfo;
        ExternalUniqueIdProperty ??= properties.FirstOrDefault(property => property.Name == $"{EntityPropertyName}{nameof(IExternalUniqueIdEntityModel.ExternalUniqueId)}")?.PropertyInfo;
    }

    private void FixupRequiredAndDeleteBehaviour()
    {
        IsRequired ??= !(ExternalUniqueIdProperty ?? UniqueIdProperty ?? EntityProperty)?.PropertyType.IsNullable() ?? false;
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
    public PropertyInfo? UniqueIdProperty { get; set; }
    public PropertyInfo? ExternalUniqueIdProperty { get; set; }
}
