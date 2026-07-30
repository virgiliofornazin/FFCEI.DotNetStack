namespace FFCEI.DotNetStack.Framework;

public static class MultipleStringsReadOnlySpanExtensionMethods
{
    public static long Count(this MultipleStringsReadOnlySpan span)
        => span.Length;

    public static char? FirstOrDefault(this MultipleStringsReadOnlySpan span, Func<char, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        for (var index = 0; index < span.Length; ++index)
        {
            var item = span[index];

            if (predicate(item))
            {
                return item;
            }
        }

        return default;
    }

    public static char First(this MultipleStringsReadOnlySpan span, Func<char, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        for (var index = 0; index < span.Length; ++index)
        {
            var item = span[index];

            if (predicate(item))
            {
                return item;
            }
        }

        throw new InvalidOperationException("Span contain no elements matching predicate");
    }

    public static char? LastOrDefault(this MultipleStringsReadOnlySpan span, Func<char, bool> predicate)
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

    public static char Last(this MultipleStringsReadOnlySpan span, Func<char, bool> predicate)
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

    public static bool Any(this MultipleStringsReadOnlySpan span, Func<char, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        for (var index = 0; index < span.Length; ++index)
        {
            var item = span[index];

            if (predicate(item))
            {
                return true;
            }
        }

        return false;
    }

#pragma warning disable CA1002 // Do not expose generic lists
    public static List<char> ToList(this MultipleStringsReadOnlySpan span)
#pragma warning restore CA1002 // Do not expose generic lists
        => span.Where(_ => true);

#pragma warning disable CA1002 // Do not expose generic lists
    public static List<char> Where(this MultipleStringsReadOnlySpan span, Func<char, bool> predicate)
#pragma warning restore CA1002 // Do not expose generic lists
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        var result = new List<char>();

        for (var index = 0; index < span.Length; ++index)
        {
            var item = span[index];

            if (predicate(item))
            {
                result.Add(item);
            }
        }

        return result;
    }
}
