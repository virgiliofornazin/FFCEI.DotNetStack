namespace FFCEI.DotNetStack.Framework;

public static class ExceptionProtector
{
    public static void ExecuteAction(Action action)
    {
        ArgumentNullException.ThrowIfNull(action, nameof(action));

        try
        {
            action();
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
        }
    }

    public static async Task ExecuteTaskAsync(Task task)
    {
        ArgumentNullException.ThrowIfNull(task, nameof(task));

        try
        {
            await task.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
        }
    }

    public static async ValueTask ExecuteValueTaskAsync(ValueTask task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
        }
    }

    public static async Task ExecuteTaskFunctionAsync(Func<Task> taskFunc)
    {
        ArgumentNullException.ThrowIfNull(taskFunc, nameof(taskFunc));

        try
        {
            var task = taskFunc();

            await task.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
        }
    }

    public static async ValueTask ExecuteValueTaskFunctionAsync(Func<ValueTask> taskFunc)
    {
        ArgumentNullException.ThrowIfNull(taskFunc, nameof(taskFunc));

        try
        {
            var task = taskFunc();

            await task.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
        }
    }

    public static T? ExecuteFunctionAndReturnOrNullValue<T>(Func<T> func) where T : struct
    {
        ArgumentNullException.ThrowIfNull(func, nameof(func));

        try
        {
            return func();
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return null;
        }
    }

    public static T? ExecuteFunctionAndReturnOrNullReference<T>(Func<T?> func) where T : class
    {
        ArgumentNullException.ThrowIfNull(func, nameof(func));

        try
        {
            return func();
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return null;
        }
    }

    public static T? ExecuteFunctionAndReturnDefaultValue<T>(Func<T> func, T? defaultResult = default) where T : struct
    {
        ArgumentNullException.ThrowIfNull(func, nameof(func));

        try
        {
            return func();
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return defaultResult;
        }
    }

    public static T? ExecuteFunctionAndReturnDefaultReference<T>(Func<T> func, T? defaultResult = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(func, nameof(func));

        try
        {
            return func();
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return defaultResult;
        }
    }

    public static async Task<T?> ExecuteTaskAndReturnOrNullValue<T>(Task<T> task) where T : struct
    {
        ArgumentNullException.ThrowIfNull(task, nameof(task));

        try
        {
            return await task.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return null;
        }
    }

    public static async Task<T?> ExecuteTaskAndReturnOrNullReference<T>(Task<T?> task) where T : class
    {
        ArgumentNullException.ThrowIfNull(task, nameof(task));

        try
        {
            return await task.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return null;
        }
    }

    public static async Task<T?> ExecuteTaskAndReturnDefaultValue<T>(Task<T> task, T? defaultResult = default) where T : struct
    {
        ArgumentNullException.ThrowIfNull(task, nameof(task));

        try
        {
            return await task.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return defaultResult;
        }
    }

    public static async Task<T?> ExecuteTaskAndReturnDefaultReference<T>(Task<T> task, T? defaultResult = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(task, nameof(task));

        try
        {
            return await task.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return defaultResult;
        }
    }

    public static async ValueTask<T?> ExecuteValueTaskAndReturnOrNullValue<T>(ValueTask<T> valuetask) where T : struct
    {
        try
        {
            return await valuetask.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return null;
        }
    }

    public static async ValueTask<T?> ExecuteValueTaskAndReturnOrNullReference<T>(ValueTask<T?> valuetask) where T : class
    {
        try
        {
            return await valuetask.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return null;
        }
    }

    public static async ValueTask<T?> ExecuteValueTaskAndReturnDefaultValue<T>(ValueTask<T> valuetask, T? defaultResult = default) where T : struct
    {
        try
        {
            return await valuetask.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return defaultResult;
        }
    }

    public static async ValueTask<T?> ExecuteValueTaskAndReturnDefaultReference<T>(ValueTask<T> valuetask, T? defaultResult = default) where T : class
    {
        try
        {
            return await valuetask.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return defaultResult;
        }
    }

    public static async Task<T?> ExecuteTaskFunctionAndReturnOrNullValue<T>(Func<Task<T>> taskFunc) where T : struct
    {
        ArgumentNullException.ThrowIfNull(taskFunc, nameof(taskFunc));

        try
        {
            var task = taskFunc();

            return await task.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return null;
        }
    }

    public static async Task<T?> ExecuteTaskFunctionAndReturnOrNullReference<T>(Func<Task<T?>> taskFunc) where T : class
    {
        ArgumentNullException.ThrowIfNull(taskFunc, nameof(taskFunc));

        try
        {
            var task = taskFunc();

            return await task.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return null;
        }
    }

    public static async Task<T?> ExecuteTaskFunctionAndReturnDefaultValue<T>(Func<Task<T>> taskFunc, T? defaultResult = default) where T : struct
    {
        ArgumentNullException.ThrowIfNull(taskFunc, nameof(taskFunc));

        try
        {
            var task = taskFunc();

            return await task.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return defaultResult;
        }
    }

    public static async Task<T?> ExecuteTaskFunctionAndReturnDefaultReference<T>(Func<Task<T>> taskFunc, T? defaultResult = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(taskFunc, nameof(taskFunc));

        try
        {
            var task = taskFunc();

            return await task.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return defaultResult;
        }
    }

    public static async ValueTask<T?> ExecuteValueTaskFunctionAndReturnOrNullValue<T>(Func<ValueTask<T>> valueTaskFunc) where T : struct
    {
        ArgumentNullException.ThrowIfNull(valueTaskFunc, nameof(valueTaskFunc));

        try
        {
            var valueTask = valueTaskFunc();

            return await valueTask.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return null;
        }
    }

    public static async ValueTask<T?> ExecuteValueTaskFunctionAndReturnOrNullReference<T>(Func<ValueTask<T?>> valueTaskFunc) where T : class
    {
        ArgumentNullException.ThrowIfNull(valueTaskFunc, nameof(valueTaskFunc));

        try
        {
            var valueTask = valueTaskFunc();

            return await valueTask.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return null;
        }
    }

    public static async ValueTask<T?> ExecuteValueTaskFunctionAndReturnDefaultValue<T>(Func<ValueTask<T>> valueTaskFunc, T? defaultResult = default) where T : struct
    {
        ArgumentNullException.ThrowIfNull(valueTaskFunc, nameof(valueTaskFunc));

        try
        {
            var valueTask = valueTaskFunc();

            return await valueTask.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return defaultResult;
        }
    }

    public static async ValueTask<T?> ExecuteValueTaskFunctionAndReturnDefaultReference<T>(Func<ValueTask<T>> valueTaskFunc, T? defaultResult = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(valueTaskFunc, nameof(valueTaskFunc));

        try
        {
            var valueTask = valueTaskFunc();

            return await valueTask.ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return defaultResult;
        }
    }
}
