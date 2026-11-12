using FFCEI.DotNetStack.EntityModels.Interfaces;
using FFCEI.DotNetStack.Framework;

namespace FFCEI.DotNetStack.EntityModels.Reflection;

internal sealed class EntityModelPropertyMapCache
    : ConcurrentKeyedObjectCache<string, EntityModelPropertyMap>
{
    private static readonly Lazy<EntityModelPropertyMapCache> _lazyInstance = new(() => new());

    internal static EntityModelPropertyMapCache Instance => _lazyInstance.Value;

    internal EntityModelPropertyMapCache()
    {
    }

    protected override bool IsValidKey(string key)
        => !string.IsNullOrEmpty(key);

    public EntityModelPropertyMap GetOrAdd<TEntityModel>(TEntityModel entityModel) where TEntityModel : class, IEntityModel
        => GetOrAdd(entityModel.GetType());

    public EntityModelPropertyMap GetOrAdd(Type entityModelType)
    {
        ArgumentNullException.ThrowIfNull(entityModelType, nameof(entityModelType));

        return GetOrAdd(entityModelType.AssemblyQualifiedName ?? string.Empty, (_) => new(entityModelType));
    }
}
