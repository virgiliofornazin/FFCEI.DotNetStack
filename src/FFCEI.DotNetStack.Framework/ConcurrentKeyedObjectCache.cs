using System.Collections.Concurrent;

namespace FFCEI.DotNetStack.Framework;

public abstract class ConcurrentKeyedObjectCache<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TValue> _itemCache = new();

    protected virtual bool IsValidKey(TKey key)
        => true;

    public bool TryGetValue(TKey key, out TValue? value)
    {
        if (!IsValidKey(key))
        {
            value = default;

            return false;
        }

        return _itemCache.TryGetValue(key, out value);
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        if (!IsValidKey(key))
        {
            return expression(key);
        }

        return _itemCache.GetOrAdd(key, (key) => expression(key));
    }

    public async ValueTask<TValue> GetOrAddTaskAsync(TKey key, Task<TValue> task)
    {
        ArgumentNullException.ThrowIfNull(task, nameof(task));

        if (!IsValidKey(key))
        {
            var result = await task.ConfigureAwait(false);

            return result;
        }

        return await _itemCache.GetOrAddTaskAsync(key, task).ConfigureAwait(false);
    }

    public async ValueTask<TValue> GetOrAddValueTaskAsync(TKey key, ValueTask<TValue> task)
    {
        if (!IsValidKey(key))
        {
            var result = await task.ConfigureAwait(false);

            return result;
        }

        return await _itemCache.GetOrAddValueTaskAsync(key, task).ConfigureAwait(false);
    }

    public async ValueTask<TValue> GetOrAddTaskFunctionAsync(TKey key, Func<TKey, Task<TValue>> func)
    {
        ArgumentNullException.ThrowIfNull(func, nameof(func));

        if (!IsValidKey(key))
        {
            var result = await func(key).ConfigureAwait(false);

            return result;
        }

        return await _itemCache.GetOrAddTaskFunctionAsync(key, func).ConfigureAwait(false);
    }

    public async ValueTask<TValue> GetOrAddValueTaskFunctionAsync(TKey key, Func<TKey, ValueTask<TValue>> func)
    {
        ArgumentNullException.ThrowIfNull(func, nameof(func));

        if (!IsValidKey(key))
        {
            var result = await func(key).ConfigureAwait(false);

            return result;
        }

        return await _itemCache.GetOrAddValueTaskFunctionAsync(key, func).ConfigureAwait(false);
    }

    public IReadOnlyCollection<TKey> Keys()
        => [.. _itemCache.Keys];

    public IReadOnlyCollection<TValue> Values()
        => [.. _itemCache.Values];

    public IReadOnlyCollection<KeyValuePair<TKey, TValue>> Items()
        => [.. _itemCache];
}
