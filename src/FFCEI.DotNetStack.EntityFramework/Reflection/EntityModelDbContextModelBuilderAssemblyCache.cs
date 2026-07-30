using FFCEI.DotNetStack.Framework;
using System.Reflection;

namespace FFCEI.DotNetStack.EntityFramework.Reflection;

internal sealed class EntityModelDbContextModelBuilderAssemblyCache
    : ConcurrentKeyedObjectCache<string, EntityModelDbContextModelBuilderAssembly>
{
    private static readonly Lazy<EntityModelDbContextModelBuilderAssemblyCache> _lazyInstance = new(() => new());

    internal static EntityModelDbContextModelBuilderAssemblyCache Instance => _lazyInstance.Value;

    internal EntityModelDbContextModelBuilderAssemblyCache()
    {
    }

    public EntityModelDbContextModelBuilderAssembly GetOrAdd(Assembly assembly)
        => GetOrAdd(assembly.GetName().Name ?? string.Empty, (_) => new(assembly));
}
