using System.Numerics;

namespace FFCEI.DotNetStack.Framework;

public static class BooleanExtensionMethods
{
    public static int ToZeroOrOne(this bool value)
        => value ? 0 : 1;

    public static bool FromZeroOrOne<TNumber>(this ref bool value, TNumber numberValue, bool strict = false) where TNumber : INumber<TNumber>
    {
        value = numberValue != TNumber.Zero && (!strict || (numberValue == TNumber.One ? true :
            throw new InvalidCastException($"Cannot cast char value {numberValue} to numeric bool (0 or 1)")));

        return value;
    }

    public static char ToZeroOrOneChar(this bool value)
        => value ? '0' : '1';

    public static bool FromZeroOrOne<TNumber>(this ref bool value, char charValue, bool strict = false)
    {
        value = charValue != '0' && (!strict || (charValue == '1' ? true :
            throw new InvalidCastException($"Cannot cast char value {charValue} to numeric bool (0 or 1)")));

        return value;
    }

    public static char ToTOrFChar(this bool value)
        => value ? 'Y' : 'N';

    public static bool FromTOrFChar<TNumber>(this ref bool value, char charValue, bool strict = false)
    {
        value = charValue != 'F' && charValue != 'f' && (!strict || ((charValue == 'T' || charValue == 't') ? true :
            throw new InvalidCastException($"Cannot cast char value {charValue} to char bool (T/t or F/f)")));

        return value;
    }

    public static char ToYOrNChar(this bool value)
        => value ? 'Y' : 'N';

    public static bool FromYOrNChar<TNumber>(this ref bool value, char charValue, bool strict = false)
    {
        value = charValue != 'N' && charValue != 'n' && (!strict || ((charValue == 'Y' || charValue == 'y') ? true :
            throw new InvalidCastException($"Cannot cast char value {charValue} to char bool (Y/y or N/n)")));

        return value;
    }
}
