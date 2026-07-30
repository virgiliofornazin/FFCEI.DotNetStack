namespace FFCEI.DotNetStack.Framework;

public static class DateTimeAlgorithms
{
    public const long TicksPerSecond = NanosecondsPerSecond / NanosecondsPerTick;
    public const long TicksPerMillisecond = TicksPerSecond / 1000L;
    public const long TicksPerMicrosecond = TicksPerMillisecond / 1000L;
    public const long NanosecondsPerTick = 100L;
    public const long NanosecondsPerSecond = 1000000000L;
    public const long SecondsPerDay = 86400L;
    public const long NanosecondsPerDay = SecondsPerDay * NanosecondsPerSecond;
    public const long MicrosecondsPerDay = NanosecondsPerDay / 1000L;
    public const long MillisecondsPerDay = MicrosecondsPerDay / 1000L;

    private const ulong TicksPerMillisecondModulus = TicksPerMillisecond;
    private const ulong TicksPerMicrosecondModulus = TicksPerMicrosecond;
    private const ulong TicksPerSecondModulus = TicksPerSecond;

    public static TimeOnly TruncateToSeconds(this TimeOnly value)
        => new(MathAlgorithms.TruncateToModulus(value.Ticks, TicksPerSecondModulus));

    public static TimeOnly TruncateToMilliseconds(this TimeOnly value)
        => new(MathAlgorithms.TruncateToModulus(value.Ticks, TicksPerMillisecondModulus));

    public static TimeOnly TruncateToMicroseconds(this TimeOnly value)
        => new(MathAlgorithms.TruncateToModulus(value.Ticks, TicksPerMicrosecondModulus));

    public static DateTime TruncateToSeconds(this DateTime value)
        => new(MathAlgorithms.TruncateToModulus(value.Ticks, TicksPerSecondModulus), value.Kind);

    public static DateTime TruncateToMilliseconds(this DateTime value)
        => new(MathAlgorithms.TruncateToModulus(value.Ticks, TicksPerMillisecondModulus), value.Kind);

    public static DateTime TruncateToMicroseconds(this DateTime value)
        => new(MathAlgorithms.TruncateToModulus(value.Ticks, TicksPerMicrosecondModulus), value.Kind);

    public static DateTimeOffset TruncateToSeconds(this DateTimeOffset value)
        => new(MathAlgorithms.TruncateToModulus(value.Ticks, TicksPerSecondModulus), value.Offset);

    public static DateTimeOffset TruncateToMilliseconds(this DateTimeOffset value)
        => new(MathAlgorithms.TruncateToModulus(value.Ticks, TicksPerMillisecondModulus), value.Offset);

    public static DateTimeOffset TruncateToMicroseconds(this DateTimeOffset value)
        => new(MathAlgorithms.TruncateToModulus(value.Ticks, TicksPerMicrosecondModulus), value.Offset);
}
