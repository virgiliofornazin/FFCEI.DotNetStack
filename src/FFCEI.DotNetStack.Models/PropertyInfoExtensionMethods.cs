using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Models.Interfaces;
using System.Reflection;

namespace FFCEI.DotNetStack.Models;

public static class PropertyInfoExtensionMethods
{
    public static bool IsEntityModelNavigation(this PropertyInfo propertyInfo)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        return propertyInfo.PropertyType.IsEntityModelNavigation();
    }

    public static bool IsEntityModelNavigationCollection(this PropertyInfo propertyInfo)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        return propertyInfo.PropertyType.IsEntityModelNavigationCollection();
    }

    public static bool IsWriteableEntityModelNavigationCollection(this PropertyInfo propertyInfo)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        return propertyInfo.PropertyType.IsWriteableEntityModelNavigationCollection();
    }

    private static readonly Type _externalUniqueIdPropertyType =
        typeof(IExternalUniqueIdEntityModel).GetProperty(nameof(IExternalUniqueIdEntityModel.ExternalUniqueId))!.PropertyType;

    private static readonly Type _idPropertyType =
        typeof(IUniqueIdEntityModel).GetProperty(nameof(IUniqueIdEntityModel.Id))!.PropertyType;

    private static bool HasForeignKeySuffix(string propertyName, string suffix)
        => (propertyName.Length > suffix.Length) && propertyName.EndsWith(suffix, StringComparison.Ordinal);

    public static bool IsIdOrExternalUniqueIdForeignKeyProperty(this PropertyInfo propertyInfo)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        if (HasForeignKeySuffix(propertyInfo.Name, nameof(IExternalUniqueIdEntityModel.ExternalUniqueId)))
        {
            var propertyType = propertyInfo.PropertyType;

            return propertyType.NonNullableType() == _externalUniqueIdPropertyType;
        }

        if (HasForeignKeySuffix(propertyInfo.Name, nameof(IUniqueIdEntityModel.Id)))
        {
            var propertyType = propertyInfo.PropertyType;

            return propertyType.NonNullableType() == _idPropertyType;
        }

        return false;
    }

    public static TAttribute? GetPropertyCustomAttributeFromEntityModelImplementationsOrInterface<TAttribute>(this PropertyInfo propertyInfo) where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        var attribute = propertyInfo.GetCustomAttribute<TAttribute>(inherit: true);

        if (attribute != null)
        {
            return attribute;
        }

        var declaringType = propertyInfo.DeclaringType;

        if (declaringType == null)
        {
            return null;
        }

        var entityModelInterfaces = declaringType.GetInterfaces().Where(interfaceType => interfaceType.IsEntityModelDerivedType());

        foreach (var entityModelInterface in entityModelInterfaces)
        {
            var interfaceProperty = entityModelInterface.GetProperty(propertyInfo.Name);

            if (interfaceProperty != null)
            {
                attribute = interfaceProperty.GetCustomAttribute<TAttribute>();

                if (attribute != null)
                {
                    break;
                }
            }
        }

        return attribute;
    }

    public static IReadOnlyCollection<TAttribute> GetPropertyCustomAttributesFromEntityModelImplementationsOrInterface<TAttribute>(this PropertyInfo propertyInfo) where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        var attributes = propertyInfo.GetCustomAttributes<TAttribute>(inherit: true).ToList();

        var declaringType = propertyInfo.DeclaringType;

        if (declaringType == null)
        {
            return attributes;
        }

        var entityModelInterfaces = declaringType.GetInterfaces().Where(interfaceType => interfaceType.IsEntityModelDerivedType());

        foreach (var entityModelInterface in entityModelInterfaces)
        {
            var interfaceProperty = entityModelInterface.GetProperty(propertyInfo.Name);

            if (interfaceProperty != null)
            {
                var interfaceAttributes = interfaceProperty.GetCustomAttributes<TAttribute>();

                attributes.AddRange(interfaceAttributes);
            }
        }

        return attributes;
    }
}
