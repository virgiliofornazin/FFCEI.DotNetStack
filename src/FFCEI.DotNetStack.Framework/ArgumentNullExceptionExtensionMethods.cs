using System.Collections;

namespace FFCEI.DotNetStack.Framework;

public static class ArgumentNullExceptionExtensionMethods
{
    public static void ThrowArgumentNullExceptionIfAny(this IEnumerable enumerable, string nameOf)
    {
        ArgumentNullException.ThrowIfNull(enumerable, nameOf);

        ulong index = 0;

        foreach (var item in enumerable)
        {
            if (item == null)
            {
                ArgumentNullException.ThrowIfNull(item, $"{nameOf}.Enumerable[{index}]");
            }

            ++index;
        }
    }
}
