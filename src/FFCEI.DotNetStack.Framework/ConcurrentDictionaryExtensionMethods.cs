using System.Collections.Concurrent;

namespace FFCEI.DotNetStack.Framework;

public static class ConcurrentDictionaryExtensionMethods
{
    public static async ValueTask<TValue> AddNewOrGetExistingAndReleaseNew<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));

        var added = dictionary.TryAdd(key, value);

        if (added)
        {
            return value;
        }

        while (true)
        {
            if (dictionary.TryGetValue(key, out var existing))
            {
                await value.DisposeObjectSafelyIfNeededAsync().ConfigureAwait(false);

                return existing;
            }

            added = dictionary.TryAdd(key, value);

            if (added)
            {
                return value;
            }
        }
    }

    public static async ValueTask<TValue> GetOrAddTaskAsync<TValue, TKey>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Task<TValue> task) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(task, nameof(task));
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));

        var value = await task.ConfigureAwait(false);

        if (dictionary.TryGetValue(key, out var existing))
        {
            if (value != null)
            {
                await value.DisposeObjectSafelyIfNeededAsync().ConfigureAwait(false);
            }

            return existing;
        }

        var result = await AddNewOrGetExistingAndReleaseNew(dictionary, key, value).ConfigureAwait(false);

        return result;
    }

    public static async ValueTask<TValue> GetOrAddValueTaskAsync<TValue, TKey>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, ValueTask<TValue> valueTask) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));

        var value = await valueTask.ConfigureAwait(false);

        if (dictionary.TryGetValue(key, out var existing))
        {
            if (value != null)
            {
                await value.DisposeObjectSafelyIfNeededAsync().ConfigureAwait(false);
            }

            return existing;
        }

        var result = await AddNewOrGetExistingAndReleaseNew(dictionary, key, value).ConfigureAwait(false);

        return result;
    }

    public static async ValueTask<TValue> GetOrAddTaskFunctionAsync<TValue, TKey>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, Task<TValue>> factory) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));

        if (dictionary.TryGetValue(key, out var existing))
        {
            return existing;
        }

        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        var value = await factory(key).ConfigureAwait(false);
        var result = await AddNewOrGetExistingAndReleaseNew(dictionary, key, value).ConfigureAwait(false);

        return result;
    }

    public static async ValueTask<TValue> GetOrAddValueTaskFunctionAsync<TValue, TKey>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, ValueTask<TValue>> factory) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));

        if (dictionary.TryGetValue(key, out var existing))
        {
            return existing;
        }

        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        var value = await factory(key).ConfigureAwait(false);
        var result = await AddNewOrGetExistingAndReleaseNew(dictionary, key, value).ConfigureAwait(false);

        return result;
    }

    public static TValue? TryGetOrAddValue<TValue, TKey>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> factory) where TKey : notnull where TValue : struct
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        return ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => dictionary.GetOrAdd(key, factory));
    }

    public static TValue? TryGetOrAddReference<TValue, TKey>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> factory) where TKey : notnull where TValue : class
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        return ExceptionProtector.ExecuteFunctionAndReturnOrNullReference(() => dictionary.GetOrAdd(key, factory));
    }

    public static async ValueTask<TValue?> TryGetOrAddTaskValueAsync<TValue, TKey>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Task<TValue> task) where TKey : notnull where TValue : struct
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));

        var result = await ExceptionProtector.ExecuteValueTaskFunctionAndReturnOrNullValue(async () =>
        {
            var value = await task.ConfigureAwait(false);

            if (dictionary.TryGetValue(key, out var existing))
            {
                await value.DisposeObjectSafelyIfNeededAsync().ConfigureAwait(false);

                return existing;
            }

            return await AddNewOrGetExistingAndReleaseNew(dictionary, key, value).ConfigureAwait(false);
        })
        .ConfigureAwait(false);

        return result;
    }

    public static async ValueTask<TValue?> TryGetOrAddTaskReferenceAsync<TValue, TKey>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Task<TValue> task) where TKey : notnull where TValue : class
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));

        var result = await ExceptionProtector.ExecuteValueTaskFunctionAndReturnOrNullReference(async () =>
        {
            var value = await task.ConfigureAwait(false);

            if (dictionary.TryGetValue(key, out var existing))
            {
                await value.DisposeObjectSafelyIfNeededAsync().ConfigureAwait(false);

                return existing;
            }

            return await AddNewOrGetExistingAndReleaseNew(dictionary, key, value).ConfigureAwait(false);
        })
        .ConfigureAwait(false);

        return result;
    }

    public static async ValueTask<TValue?> TryGetOrAddValueTaskValueAsync<TValue, TKey>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, ValueTask<TValue> task) where TKey : notnull where TValue : struct
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));

        var result = await ExceptionProtector.ExecuteValueTaskFunctionAndReturnOrNullValue(async () =>
        {
            var value = await task.ConfigureAwait(false);

            if (dictionary.TryGetValue(key, out var existing))
            {
                await value.DisposeObjectSafelyIfNeededAsync().ConfigureAwait(false);

                return existing;
            }

            return await AddNewOrGetExistingAndReleaseNew(dictionary, key, value).ConfigureAwait(false);
        })
        .ConfigureAwait(false);

        return result;
    }

    public static async ValueTask<TValue?> TryGetOrAddValueTaskReferenceAsync<TValue, TKey>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, ValueTask<TValue> task) where TKey : notnull where TValue : class
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));

        var result = await ExceptionProtector.ExecuteValueTaskFunctionAndReturnOrNullReference(async () =>
        {
            var value = await task.ConfigureAwait(false);

            if (dictionary.TryGetValue(key, out var existing))
            {
                await value.DisposeObjectSafelyIfNeededAsync().ConfigureAwait(false);

                return existing;
            }

            return await AddNewOrGetExistingAndReleaseNew(dictionary, key, value).ConfigureAwait(false);
        })
        .ConfigureAwait(false);

        return result;
    }

    public static async ValueTask<TValue?> TryGetOrAddTaskValueFunctionAsync<TValue, TKey>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, Task<TValue>> factory) where TKey : notnull where TValue : struct
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));

        if (dictionary.TryGetValue(key, out var existing))
        {
            return existing;
        }

        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        var result = await ExceptionProtector.ExecuteValueTaskFunctionAndReturnOrNullValue(async () =>
        {
            if (dictionary.TryGetValue(key, out var existing))
            {
                return existing;
            }

            var value = await factory(key).ConfigureAwait(false);

            return await AddNewOrGetExistingAndReleaseNew(dictionary, key, value).ConfigureAwait(false);
        })
        .ConfigureAwait(false);

        return result;
    }

    public static async ValueTask<TValue?> TryGetOrAddTaskReferenceFunctionAsync<TValue, TKey>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, Task<TValue>> factory) where TKey : notnull where TValue : class
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));

        if (dictionary.TryGetValue(key, out var existing))
        {
            return existing;
        }

        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        var result = await ExceptionProtector.ExecuteValueTaskFunctionAndReturnOrNullReference(async () =>
        {
            if (dictionary.TryGetValue(key, out var existing))
            {
                return existing;
            }

            var value = await factory(key).ConfigureAwait(false);

            return await AddNewOrGetExistingAndReleaseNew(dictionary, key, value).ConfigureAwait(false);
        })
        .ConfigureAwait(false);

        return result;
    }

    public static async ValueTask<TValue?> TryGetOrAddValueTaskValueFunctionAsync<TValue, TKey>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, ValueTask<TValue>> factory) where TKey : notnull where TValue : struct
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));

        if (dictionary.TryGetValue(key, out var existing))
        {
            return existing;
        }

        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        var result = await ExceptionProtector.ExecuteValueTaskFunctionAndReturnOrNullValue(async () =>
        {
            var value = await factory(key).ConfigureAwait(false);

            if (dictionary.TryGetValue(key, out var existing))
            {
                await value.DisposeObjectSafelyIfNeededAsync().ConfigureAwait(false);

                return existing;
            }

            return await AddNewOrGetExistingAndReleaseNew(dictionary, key, value).ConfigureAwait(false);
        })
        .ConfigureAwait(false);

        return result;
    }

    public static async ValueTask<TValue?> TryGetOrAddValueTaskReferenceFunctionAsync<TValue, TKey>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, ValueTask<TValue>> factory) where TKey : notnull where TValue : class
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));

        if (dictionary.TryGetValue(key, out var existing))
        {
            return existing;
        }

        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        var result = await ExceptionProtector.ExecuteValueTaskFunctionAndReturnOrNullReference(async () =>
        {
            var value = await factory(key).ConfigureAwait(false);

            if (dictionary.TryGetValue(key, out var existing))
            {
                await value.DisposeObjectSafelyIfNeededAsync().ConfigureAwait(false);

                return existing;
            }

            return await AddNewOrGetExistingAndReleaseNew(dictionary, key, value).ConfigureAwait(false);
        })
        .ConfigureAwait(false);

        return result;
    }
}
