using System.Numerics;

namespace FFCEI.DotNetStack.Framework;

public static class MathAlgorithms
{
    public static T TruncateToModulus<T, TM>(T value, TM modulus) where T : IBinaryInteger<T> where TM : IBinaryInteger<TM>, IUnsignedNumber<TM>
    {
        var targetModulus = T.CreateChecked(modulus);

        return value - (value % targetModulus);
    }
}
