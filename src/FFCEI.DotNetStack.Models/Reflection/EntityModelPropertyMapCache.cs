using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models.Reflection;

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

    public EntityModelPropertyMap GetOrAdd<TEntityModel>(TEntityModel model) where TEntityModel : class, IEntityModel
        => GetOrAdd(model.GetType());

    public EntityModelPropertyMap GetOrAdd(Type entityModelType)
    {
        ArgumentNullException.ThrowIfNull(entityModelType, nameof(entityModelType));

        return GetOrAdd(entityModelType.AssemblyQualifiedName ?? string.Empty, (_) => new(entityModelType));
    }
}
