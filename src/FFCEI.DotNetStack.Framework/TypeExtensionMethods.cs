using System.Collections.Concurrent;
using System.Reflection;

namespace FFCEI.DotNetStack.Framework;

public static class TypeExtensionMethods
{
    private static readonly Lazy<Dictionary<Type, string>> _primitiveCSharpTypesMap = new(() =>
        new() {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(object), "object" },
            { typeof(string), "string" },
            { typeof(void), "void" }
        });

    public static string NamespaceAndName(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        return $"{type.Namespace}.{type.Name}";
    }

    public static Type? FindType(string typeName)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(typeName);

            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    public static string CSharpTypeName(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        if (type.IsNullable())
        {
            var underlyingType = Nullable.GetUnderlyingType(type);

            if (underlyingType != null)
            {
                return $"{CSharpTypeName(underlyingType)}?";
            }
        }

        if (_primitiveCSharpTypesMap.Value.TryGetValue(type, out var alias))
        {
            return alias;
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType();

            if (elementType != null)
            {
                var rankIndicators = new string(',', type.GetArrayRank() - 1);

                return $"{CSharpTypeName(elementType)}[{rankIndicators}]";
            }
        }

        if (type.IsGenericType)
        {
            var backtickIndex = type.Name.IndexOf('`', StringComparison.Ordinal);
            var baseName = backtickIndex >= 0 ? type.Name[..backtickIndex] : type.Name;
            var declaringPrefix = type.IsNested && type.DeclaringType != null ? $"{CSharpTypeName(type.DeclaringType)}." : string.Empty;

            if (type.IsGenericTypeDefinition)
            {
                return $"{declaringPrefix}{baseName}<{new string(',', type.GetGenericArguments().Length - 1)}>";
            }
            else
            {
                var genericArgs = type.GetGenericArguments().Select(CSharpTypeName);

                return $"{declaringPrefix}{baseName}<{string.Join(", ", genericArgs)}>";
            }
        }

        return type.IsNested && type.DeclaringType != null ? $"{CSharpTypeName(type.DeclaringType)}.{type.Name}" : type.Name;
    }

    public static bool IsStruct(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        return type.IsValueType && !type.IsEnum && !type.IsByRefLike;
    }

    public static bool IsRefStruct(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        return type.IsByRefLike;
    }

    public static bool IsNullable(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    public static Type NonNullableType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        return type.IsNullable() ? Nullable.GetUnderlyingType(type) ?? type : type;
    }

    public static bool IsNullAssignable(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        return !type.IsValueType || type.IsNullable();
    }

    public static bool IsStrictlyMoreDerived(this Type? derivedType, Type? baseType)
        => ((derivedType == null) || (baseType == null) || (derivedType == baseType)) ? false : derivedType.IsAssignableTo(baseType);

    private static readonly ConcurrentDictionary<Type, IReadOnlyCollection<Type>> _inheritanceHierarchyCache = new();

    public static IReadOnlyCollection<Type> InheritanceHierarchy(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        var inheritanceHierarchy = _inheritanceHierarchyCache.GetOrAdd(type, (_) =>
        {
            var hierarchy = new List<Type>();

            for (var current = type; current != null; current = current.BaseType)
            {
                hierarchy.Add(current);
            }

            return hierarchy.AsReadOnly();
        });

        return inheritanceHierarchy;
    }

    private static readonly ConcurrentDictionary<Type, IReadOnlyCollection<Type>> _interfaceInheritanceHierarchyCache = new();

    public static IReadOnlyCollection<Type> GetInterfaceHierarchy(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        var inheritanceHierarchy = _interfaceInheritanceHierarchyCache.GetOrAdd(type, (_) =>
        {
            var interfaces = type.GetInterfaces();
            var hierarchy = new List<Type>();

            if (interfaces.Length != 0)
            {
                var interfaceSet = new HashSet<Type>(interfaces);
                var visited = new HashSet<Type>();
                var stack = new Stack<(Type Type, bool Processed)>();

                for (int i = interfaces.Length - 1; i >= 0; i--)
                {
                    stack.Push((interfaces[i], false));
                }

                while (stack.Count > 0)
                {
                    var (type, processed) = stack.Pop();

                    if (visited.Contains(type))
                    {
                        continue;
                    }

                    if (processed)
                    {
                        var unused = visited.Add(type);

                        hierarchy.Add(type);

                        continue;
                    }

                    stack.Push((type, true));

                    var parents = type.GetInterfaces();

                    for (int i = parents.Length - 1; i >= 0; i--)
                    {
                        var parent = parents[i];

                        if (interfaceSet.Contains(parent) && !visited.Contains(parent))
                        {
                            stack.Push((parent, false));
                        }
                    }
                }
            }

            return hierarchy.AsReadOnly();
        });

        return inheritanceHierarchy;
    }

    public static bool Implements<TType>(this Type type)
        => type.Implements([typeof(TType)]);

    public static bool Implements(this Type type, params ReadOnlySpan<Type> types)
        => type.ImplementsAll(types);

    public static bool ImplementsAll(this Type type, params ReadOnlySpan<Type> types)
        => type.ImplementedTypes(types).Count == types.Length;

    public static bool ImplementsAny(this Type type, params ReadOnlySpan<Type> types)
        => type.ImplementedTypes(types).Count > 0;

    public static IReadOnlyCollection<Type> ImplementedTypes(this Type type, params ReadOnlySpan<Type> types)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        var implementedTypes = types.Where(testingType => testingType.IsInterface ? testingType.IsAssignableFrom(type) :
            type.InheritanceHierarchy().Any(inheritedType => inheritedType == testingType)).ToList();

        return implementedTypes.AsReadOnly();
    }

    public static bool AreInterfaces(bool throwOnFailure, params ReadOnlySpan<Type> interfaceTypes)
    {
        var list = interfaceTypes.ToList();

        if (list.Any(t => t == null || !t.IsInterface))
        {
            if (throwOnFailure)
            {
                throw new ArgumentException("All elements in the collection must be non-null interface types.", nameof(interfaceTypes));
            }

            return false;
        }

        return true;
    }
}
