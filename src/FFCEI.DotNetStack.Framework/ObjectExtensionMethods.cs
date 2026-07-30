namespace FFCEI.DotNetStack.Framework;

public static class ObjectExtensionMethods
{
    public static void DisposeObjectSafelyIfNeeded(this object? value)
    {
        if (value is IAsyncDisposable asyncDisposable)
        {
            var _ = asyncDisposable.DisposeSafelyIfNeededAsync();
        }
        else if (value is IDisposable disposable)
        {
            disposable.DisposeSafelyIfNeeded();
        }
    }

    public static ValueTask DisposeObjectSafelyIfNeededAsync(this object? value)
    {
        if (value is IAsyncDisposable asyncDisposable)
        {
            return asyncDisposable.DisposeSafelyIfNeededAsync();
        }
        else if (value is IDisposable disposable)
        {
            disposable.DisposeSafelyIfNeeded();
        }

        return ValueTask.CompletedTask;
    }

    public static void DisposeSafelyIfNeeded(this IDisposable? value)
    {
        if (value is IAsyncDisposable asyncDisposable)
        {
            ExceptionProtector.ExecuteAction(() => { var unused = asyncDisposable.DisposeAsync(); });
        }
        else if (value != null)
        {
            ExceptionProtector.ExecuteAction(() => value.Dispose());
        }
    }

    public static ValueTask DisposeSafelyIfNeededAsync(this IAsyncDisposable? value)
    {
        if (value != null)
        {
            return ExceptionProtector.ExecuteValueTaskAsync(value.DisposeAsync());
        }

        return ValueTask.CompletedTask;
    }
}
