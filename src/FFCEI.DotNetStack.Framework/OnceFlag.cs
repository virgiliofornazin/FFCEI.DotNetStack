using System.Diagnostics;

namespace FFCEI.DotNetStack.Framework;

public struct OnceFlag : IEquatable<OnceFlag>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _flag;

    public readonly bool IsSet => _flag != 0;

    public bool TestAndSet()
        => Interlocked.Exchange(ref _flag, 1) == 0;

    public readonly bool Equals(OnceFlag other)
        => _flag == other._flag;

    public override readonly bool Equals(object? obj)
        => obj is OnceFlag other && Equals(other);

    public override readonly int GetHashCode()
        => _flag.GetHashCode();

    public static bool operator ==(OnceFlag left, OnceFlag right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(OnceFlag left, OnceFlag right)
    {
        return !(left == right);
    }

}
