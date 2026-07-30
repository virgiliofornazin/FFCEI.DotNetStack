namespace FFCEI.DotNetStack.Framework;

public static class ReadOnlySpanLinqExtensionMethods
{
    public static long Count<T>(this ReadOnlySpan<T> span)
        => span.Length;

    public static T? FirstOrDefault<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        foreach (var item in span)
        {
            if (predicate(item))
            {
                return item;
            }
        }

        return default;
    }

    public static T First<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        foreach (var item in span)
        {
            if (predicate(item))
            {
                return item;
            }
        }

        throw new InvalidOperationException("Span contain no elements matching predicate");
    }

    public static T? LastOrDefault<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        for (var index = span.Length - 1; index > -1; --index)
        {
            var item = span[index];

            if (predicate(item))
            {
                return item;
            }
        }

        return default;
    }

    public static T Last<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        for (var index = span.Length - 1; index > -1; --index)
        {
            var item = span[index];

            if (predicate(item))
            {
                return item;
            }
        }

        throw new InvalidOperationException("Span contain no elements matching predicate");
    }

    public static bool Any<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        foreach (var item in span)
        {
            if (predicate(item))
            {
                return true;
            }
        }

        return false;
    }

#pragma warning disable CA1002 // Do not expose generic lists
    public static List<T> ToList<T>(this ReadOnlySpan<T> span)
#pragma warning restore CA1002 // Do not expose generic lists
        => span.Where(_ => true);

#pragma warning disable CA1002 // Do not expose generic lists
    public static List<T> Where<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
#pragma warning restore CA1002 // Do not expose generic lists
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        var result = new List<T>();

        foreach (var item in span)
        {
            if (predicate(item))
            {
                result.Add(item);
            }
        }

        return result;
    }
}
