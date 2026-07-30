using System.Globalization;

namespace FFCEI.DotNetStack.Framework;

public static class ValueConversions
{
    private const string DateTimeToStringMask = "yyyy-MM-ddTHH:mm:ss.fffffffK";
    private const string DateTimeOffsetToStringMask = "yyyy-MM-ddTHH:mm:ss.fffffffK";

    public static DateTime DateTimeAdjustToLocalIfNeededOr(DateTime value, DateTimeKind? overrideKind)
        => DateTime.SpecifyKind(value.Kind == DateTimeKind.Utc ? value.ToLocalTime() : value, overrideKind ?? DateTimeKind.Local);

    public static DateTime DateTimeAdjustToUtcIfNeededOr(DateTime value, DateTimeKind? overrideKind)
        => DateTime.SpecifyKind(value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value, overrideKind ?? DateTimeKind.Utc);

    public static DateTime DateTimeAdjustToLocalIfNeeded(DateTime value)
        => DateTimeAdjustToLocalIfNeededOr(value, DateTimeKind.Local);

    public static DateTime DateTimeAdjustToUtcIfNeeded(DateTime value)
        => DateTimeAdjustToUtcIfNeededOr(value, DateTimeKind.Utc);

    public static string DateTimeLocalToString(DateTime value, CultureInfo? cultureInfo = null)
        => DateTimeAdjustToLocalIfNeeded(value).ToString(DateTimeToStringMask, cultureInfo ?? CultureInfo.InvariantCulture);

    public static string? TryDateTimeLocalToString(DateTime? value, CultureInfo? cultureInfo = null)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullReference(() => DateTimeAdjustToLocalIfNeeded(value.Value).ToString(DateTimeToStringMask, cultureInfo ?? CultureInfo.InvariantCulture));

    public static DateTime DateTimeFromLocalString(string value, CultureInfo? cultureInfo = null)
        => DateTimeAdjustToLocalIfNeeded(DateTime.Parse(value, cultureInfo ?? CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

    public static DateTime? TryDateTimeFromLocalString(string? value, CultureInfo? cultureInfo = null)
        => string.IsNullOrWhiteSpace(value) ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeFromLocalString(value, cultureInfo));

    public static string DateTimeUtcToString(DateTime value, CultureInfo? cultureInfo = null)
        => DateTimeAdjustToUtcIfNeeded(value).ToString(DateTimeToStringMask, cultureInfo ?? CultureInfo.InvariantCulture);

    public static string? TryDateTimeUtcToString(DateTime? value, CultureInfo? cultureInfo = null)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullReference(() => DateTimeAdjustToUtcIfNeeded(value.Value).ToString(DateTimeToStringMask, cultureInfo ?? CultureInfo.InvariantCulture));

    public static DateTime DateTimeFromUtcString(string value, CultureInfo? cultureInfo = null)
        => DateTimeAdjustToUtcIfNeeded(DateTime.Parse(value, cultureInfo ?? CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

    public static DateTime? TryDateTimeFromUtcString(string? value, CultureInfo? cultureInfo = null)
        => string.IsNullOrWhiteSpace(value) ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeFromUtcString(value, cultureInfo));

    public const long DateTimeUtcSerialWithMillisecondsYearMultiplier = 10000000000000L;
    public const long DateTimeUtcSerialWithMillisecondsMonthMultiplier = 100000000000L;
    public const long DateTimeUtcSerialWithMillisecondsDayMultiplier = 1000000000L;
    public const long DateTimeUtcSerialWithMillisecondsHourMultiplier = 10000000L;
    public const long DateTimeUtcSerialWithMillisecondsMinuteMultiplier = 100000L;
    public const long DateTimeUtcSerialWithMillisecondsSecondMultiplier = 1000L;

    private static void CheckSerialDateOrTimeValue(long value)
    {
        if (value < 0)
        {
            throw new InvalidOperationException($"Invalid serial value '{value}' cannot be converted.");
        }
    }

    public static long DateTimeUtcToSerialWithMilliseconds(DateTime value)
    {
        var datetime = DateTimeAdjustToUtcIfNeeded(value);

        return (datetime.Year * DateTimeUtcSerialWithMillisecondsYearMultiplier)
             + (datetime.Month * DateTimeUtcSerialWithMillisecondsMonthMultiplier)
             + (datetime.Day * DateTimeUtcSerialWithMillisecondsDayMultiplier)
             + (datetime.Hour * DateTimeUtcSerialWithMillisecondsHourMultiplier)
             + (datetime.Minute * DateTimeUtcSerialWithMillisecondsMinuteMultiplier)
             + (datetime.Second * DateTimeUtcSerialWithMillisecondsSecondMultiplier)
             + datetime.Millisecond;
    }

    public static long? TryDateTimeUtcToSerialWithMilliseconds(DateTime? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeUtcToSerialWithMilliseconds(value.Value));

    public static DateTime DateTimeUtcFromSerialWithMilliseconds(long value)
    {
        CheckSerialDateOrTimeValue(value);

        int year = (int)(value / DateTimeUtcSerialWithMillisecondsYearMultiplier);
        int month = (int)((value % DateTimeUtcSerialWithMillisecondsYearMultiplier) / DateTimeUtcSerialWithMillisecondsMonthMultiplier);
        int day = (int)((value % DateTimeUtcSerialWithMillisecondsMonthMultiplier) / DateTimeUtcSerialWithMillisecondsDayMultiplier);
        int hour = (int)((value % DateTimeUtcSerialWithMillisecondsDayMultiplier) / DateTimeUtcSerialWithMillisecondsHourMultiplier);
        int minute = (int)((value % DateTimeUtcSerialWithMillisecondsHourMultiplier) / DateTimeUtcSerialWithMillisecondsMinuteMultiplier);
        int second = (int)((value % DateTimeUtcSerialWithMillisecondsMinuteMultiplier) / DateTimeUtcSerialWithMillisecondsSecondMultiplier);
        int millisecond = (int)(value % DateTimeUtcSerialWithMillisecondsSecondMultiplier);

        return new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
    }

    public static DateTime? TryDateTimeUtcFromSerialWithMilliseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeUtcFromSerialWithMilliseconds(value.Value));

    public const long DateTimeUtcSerialYearMultiplier = 10000000000L;
    public const long DateTimeUtcSerialMonthMultiplier = 100000000L;
    public const long DateTimeUtcSerialDayMultiplier = 1000000L;
    public const long DateTimeUtcSerialHourMultiplier = 10000L;
    public const long DateTimeUtcSerialMinuteMultiplier = 100L;

    public static long DateTimeUtcToSerial(DateTime value)
    {
        var datetime = DateTimeAdjustToUtcIfNeeded(value);

        return (datetime.Year * DateTimeUtcSerialYearMultiplier)
             + (datetime.Month * DateTimeUtcSerialMonthMultiplier)
             + (datetime.Day * DateTimeUtcSerialDayMultiplier)
             + (datetime.Hour * DateTimeUtcSerialHourMultiplier)
             + (datetime.Minute * DateTimeUtcSerialMinuteMultiplier)
             + datetime.Second;
    }

    public static long? TryDateTimeUtcToSerial(DateTime? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeUtcToSerial(value.Value));

    public static DateTime DateTimeUtcFromSerial(long value)
    {
        CheckSerialDateOrTimeValue(value);

        int year = (int)(value / DateTimeUtcSerialYearMultiplier);
        int month = (int)((value % DateTimeUtcSerialYearMultiplier) / DateTimeUtcSerialMonthMultiplier);
        int day = (int)((value % DateTimeUtcSerialMonthMultiplier) / DateTimeUtcSerialDayMultiplier);
        int hour = (int)((value % DateTimeUtcSerialDayMultiplier) / DateTimeUtcSerialHourMultiplier);
        int minute = (int)((value % DateTimeUtcSerialHourMultiplier) / DateTimeUtcSerialMinuteMultiplier);
        int second = (int)(value % DateTimeUtcSerialMinuteMultiplier);

        return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
    }

    public static DateTime? TryDateTimeUtcFromSerial(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeUtcFromSerial(value.Value));

    public static long DateTimeUtcToUnixSeconds(DateTime value)
        => DateTimeOffsetUtcToUnixSeconds(DateTimeAdjustToUtcIfNeeded(value));

    public static long? TryDateTimeUtcToUnixSeconds(DateTime? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeUtcToUnixSeconds(value.Value));

    public static DateTime DateTimeUtcFromUnixSeconds(long value)
        => DateTimeOffsetUtcFromUnixSeconds(value).UtcDateTime;

    public static DateTime? TryDateTimeUtcFromUnixSeconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeUtcFromUnixSeconds(value.Value));

    public static long DateTimeUtcToUnixMilliseconds(DateTime value)
        => DateTimeOffsetUtcToUnixMilliseconds(DateTimeAdjustToUtcIfNeeded(value));

    public static long? TryDateTimeUtcToUnixMilliseconds(DateTime? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeUtcToUnixMilliseconds(value.Value));

    public static DateTime DateTimeUtcFromUnixMilliseconds(long value)
        => DateTimeOffsetUtcFromUnixMilliseconds(value).UtcDateTime;

    public static DateTime? TryDateTimeUtcFromUnixMilliseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeUtcFromUnixMilliseconds(value.Value));

    public static DateTimeOffset DateTimeOffsetAdjustToLocalIfNeeded(DateTimeOffset value)
        => value.ToLocalTime();

    public static DateTimeOffset DateTimeOffsetAdjustToUtcIfNeeded(DateTimeOffset value)
        => value.ToUniversalTime();

    public static string DateTimeOffsetToString(DateTimeOffset value, CultureInfo? cultureInfo = null)
        => value.ToString(DateTimeOffsetToStringMask, cultureInfo ?? CultureInfo.InvariantCulture);

    public static string? TryDateTimeOffsetToString(DateTimeOffset? value, CultureInfo? cultureInfo = null)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullReference(() => DateTimeOffsetToString(value.Value, cultureInfo));

    public static DateTimeOffset DateTimeOffsetFromString(string value, CultureInfo? cultureInfo = null)
        => DateTimeOffset.Parse(value, cultureInfo ?? CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

    public static DateTimeOffset? TryDateTimeOffsetFromString(string? value, CultureInfo? cultureInfo = null)
        => string.IsNullOrWhiteSpace(value) ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeOffsetFromString(value, cultureInfo));

    public static string DateTimeOffsetLocalToString(DateTimeOffset value, CultureInfo? cultureInfo = null)
        => DateTimeOffsetAdjustToLocalIfNeeded(value).ToString(DateTimeOffsetToStringMask, cultureInfo ?? CultureInfo.InvariantCulture);

    public static string? TryDateTimeOffsetLocalToString(DateTimeOffset? value, CultureInfo? cultureInfo = null)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullReference(() => DateTimeOffsetLocalToString(value.Value, cultureInfo));

    public static DateTimeOffset DateTimeOffsetFromLocalString(string value, CultureInfo? cultureInfo = null)
        => DateTimeOffsetAdjustToLocalIfNeeded(DateTimeOffset.Parse(value, cultureInfo ?? CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

    public static DateTimeOffset? TryDateTimeOffsetFromLocalString(string? value, CultureInfo? cultureInfo = null)
        => string.IsNullOrWhiteSpace(value) ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeOffsetFromLocalString(value, cultureInfo));

    public static string DateTimeOffsetUtcToString(DateTimeOffset value, CultureInfo? cultureInfo = null)
        => DateTimeOffsetAdjustToUtcIfNeeded(value).ToString(DateTimeOffsetToStringMask, cultureInfo ?? CultureInfo.InvariantCulture);

    public static string? TryDateTimeOffsetUtcToString(DateTimeOffset? value, CultureInfo? cultureInfo = null)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullReference(() => DateTimeOffsetUtcToString(value.Value, cultureInfo));

    public static DateTimeOffset DateTimeOffsetFromUtcString(string value, CultureInfo? cultureInfo = null)
        => DateTimeOffsetAdjustToUtcIfNeeded(DateTimeOffset.Parse(value, cultureInfo ?? CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

    public static DateTimeOffset? TryDateTimeOffsetFromUtcString(string? value, CultureInfo? cultureInfo = null)
        => string.IsNullOrWhiteSpace(value) ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeOffsetFromUtcString(value, cultureInfo));

    public static long DateTimeOffsetUtcToSerialWithMilliseconds(DateTimeOffset value)
        => DateTimeUtcToSerialWithMilliseconds(DateTimeOffsetAdjustToUtcIfNeeded(value).DateTime);

    public static long? TryDateTimeOffsetUtcToSerialWithMilliseconds(DateTimeOffset? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeOffsetUtcToSerialWithMilliseconds(value.Value));

    public static DateTimeOffset DateTimeOffsetUtcFromSerialWithMilliseconds(long value)
        => DateTimeUtcFromSerialWithMilliseconds(value);

    public static DateTimeOffset? TryDateTimeOffsetUtcFromSerialWithMilliseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeOffsetUtcFromSerialWithMilliseconds(value.Value));

    public static long DateTimeOffsetUtcToSerial(DateTimeOffset value)
        => DateTimeUtcToSerial(DateTimeOffsetAdjustToUtcIfNeeded(value).DateTime);

    public static long? TryDateTimeOffsetUtcToSerial(DateTimeOffset? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeOffsetUtcToSerial(value.Value));

    public static DateTimeOffset DateTimeOffsetUtcFromSerial(long value)
        => DateTimeUtcFromSerial(value);

    public static DateTimeOffset? TryDateTimeOffsetUtcFromSerial(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeOffsetUtcFromSerial(value.Value));

    public static long DateTimeOffsetUtcToUnixSeconds(DateTimeOffset value)
        => DateTimeOffsetAdjustToUtcIfNeeded(value).ToUnixTimeSeconds();

    public static long? TryDateTimeOffsetUtcToUnixSeconds(DateTimeOffset? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeOffsetUtcToUnixSeconds(value.Value));

    public static DateTimeOffset DateTimeOffsetUtcFromUnixSeconds(long value)
        => DateTimeOffsetAdjustToUtcIfNeeded(DateTimeOffset.FromUnixTimeSeconds(value));

    public static DateTimeOffset? TryDateTimeOffsetUtcFromUnixSeconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeOffsetUtcFromUnixSeconds(value.Value));

    public static long DateTimeOffsetUtcToUnixMilliseconds(DateTimeOffset value)
        => DateTimeOffsetAdjustToUtcIfNeeded(value).ToUnixTimeMilliseconds();

    public static long? TryDateTimeOffsetUtcToUnixMilliseconds(DateTimeOffset? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeOffsetUtcToUnixMilliseconds(value.Value));

    public static DateTimeOffset DateTimeOffsetUtcFromUnixMilliseconds(long value)
        => DateTimeOffsetAdjustToUtcIfNeeded(DateTimeOffset.FromUnixTimeMilliseconds(value));

    public static DateTimeOffset? TryDateTimeOffsetUtcFromUnixMilliseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateTimeOffsetUtcFromUnixMilliseconds(value.Value));

    public const long MinTicksToNanoseconds = long.MinValue / DateTimeAlgorithms.NanosecondsPerTick;
    public const long MaxTicksToNanoseconds = long.MaxValue / DateTimeAlgorithms.NanosecondsPerTick;
    public const long MinTicksFromMicroseconds = long.MinValue / DateTimeAlgorithms.TicksPerMicrosecond;
    public const long MaxTicksFromMicroseconds = long.MaxValue / DateTimeAlgorithms.TicksPerMicrosecond;
    public const long MinTicksFromMilliseconds = long.MinValue / DateTimeAlgorithms.TicksPerMillisecond;
    public const long MaxTicksFromMilliseconds = long.MaxValue / DateTimeAlgorithms.TicksPerMillisecond;

    public static long TicksToNanoseconds(long value)
    {
        if ((value < MinTicksToNanoseconds) || (value > MaxTicksToNanoseconds))
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Ticks value out of representable nanosecond range. Value: {value}");
        }

        return value * DateTimeAlgorithms.NanosecondsPerTick;
    }

    public static long? TryTicksToNanoseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TicksToNanoseconds(value.Value));

    public static long TicksFromNanoseconds(long value)
        => value / DateTimeAlgorithms.NanosecondsPerTick;

    public static long? TryTicksFromNanoseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TicksFromNanoseconds(value.Value));

    public static long TicksToMicroseconds(long value)
        => value / DateTimeAlgorithms.TicksPerMicrosecond;

    public static long? TryTicksToMicroseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TicksToMicroseconds(value.Value));

    public static long TicksFromMicroseconds(long value)
    {
        if ((value < MinTicksFromMicroseconds) || (value > MaxTicksFromMicroseconds))
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Microseconds value out of representable tick range. Value: {value}");
        }

        return value * DateTimeAlgorithms.TicksPerMicrosecond;
    }

    public static long? TryTicksFromMicroseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TicksFromMicroseconds(value.Value));

    public static long TicksToMilliseconds(long value)
        => value / DateTimeAlgorithms.TicksPerMillisecond;

    public static long? TryTicksToMilliseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TicksToMilliseconds(value.Value));

    public static long TicksFromMilliseconds(long value)
    {
        if ((value < MinTicksFromMilliseconds) || (value > MaxTicksFromMilliseconds))
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Milliseconds value out of representable tick range. Value: {value}");
        }

        return value * DateTimeAlgorithms.TicksPerMillisecond;
    }

    public static long? TryTicksFromMilliseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TicksFromMilliseconds(value.Value));

    public static long TimeSpanToNanoseconds(TimeSpan value)
        => TicksToNanoseconds(value.Ticks);

    public static long? TryTimeSpanToNanoseconds(TimeSpan? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeSpanToNanoseconds(value.Value));

    public static TimeSpan TimeSpanFromNanoseconds(long value)
        => TimeSpan.FromTicks(TicksFromNanoseconds(value));

    public static TimeSpan? TryTimeSpanFromNanoseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeSpanFromNanoseconds(value.Value));

    public static long TimeSpanToMicroseconds(TimeSpan value)
        => TicksToMicroseconds(value.Ticks);

    public static long? TryTimeSpanToMicroseconds(TimeSpan? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeSpanToMicroseconds(value.Value));

    public static TimeSpan TimeSpanFromMicroseconds(long value)
        => TimeSpan.FromTicks(TicksFromMicroseconds(value));

    public static TimeSpan? TryTimeSpanFromMicroseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeSpanFromMicroseconds(value.Value));

    public static long TimeSpanToMilliseconds(TimeSpan value)
        => TicksToMilliseconds(value.Ticks);

    public static long? TryTimeSpanToMilliseconds(TimeSpan? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeSpanToMilliseconds(value.Value));

    public static TimeSpan TimeSpanFromMilliseconds(long value)
        => TimeSpan.FromTicks(TicksFromMilliseconds(value));

    public static TimeSpan? TryTimeSpanFromMilliseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeSpanFromMilliseconds(value.Value));

    public const int DateOnlySerialYearMultiplier = 10000;
    public const int DateOnlySerialMonthMultiplier = 100;
    public const int DateOnlySerialDayMultiplier = 1;

    public static int DateOnlyToSerial(DateOnly value)
        => (value.Year * DateOnlySerialYearMultiplier)
            + (value.Month * DateOnlySerialMonthMultiplier)
            + value.Day;

    public static int? TryDateOnlyToSerial(DateOnly? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateOnlyToSerial(value.Value));

    public static DateOnly DateOnlyFromSerial(int value)
    {
        CheckSerialDateOrTimeValue(value);

        int year = value / DateOnlySerialYearMultiplier;
        int month = (value % DateOnlySerialYearMultiplier) / DateOnlySerialMonthMultiplier;
        int day = value % DateOnlySerialMonthMultiplier;

        return new DateOnly(year, month, day);
    }

    public static DateOnly? TryDateOnlyFromSerial(int? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => DateOnlyFromSerial(value.Value));

    public static long TimeOnlyToNanoseconds(TimeOnly value)
    {
        var nanoseconds = TicksToNanoseconds(value.Ticks);

        if (nanoseconds < 0 || nanoseconds >= DateTimeAlgorithms.NanosecondsPerDay)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"The TimeOnly value must be between 00:00:00.000000000 and 23:59:59.999999999. Value: {value}");
        }

        return nanoseconds;
    }

    public static long? TryTimeOnlyToNanoseconds(TimeOnly? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeOnlyToNanoseconds(value.Value));

    public static TimeOnly TimeOnlyFromNanoseconds(long value)
    {
        if (value < 0 || value >= DateTimeAlgorithms.NanosecondsPerDay)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"The TimeOnly value must be between 0 and {DateTimeAlgorithms.NanosecondsPerDay - 1}. Value: {value}");
        }

        return TimeOnly.FromTimeSpan(TimeSpan.FromTicks(TicksFromNanoseconds(value)));
    }

    public static TimeOnly? TryTimeOnlyFromNanoseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeOnlyFromNanoseconds(value.Value));

    public static long TimeOnlyToMicroseconds(TimeOnly value)
    {
        var microseconds = TicksToMicroseconds(value.Ticks);

        if (microseconds < 0 || microseconds >= DateTimeAlgorithms.MicrosecondsPerDay)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"The TimeOnly value must be between 00:00:00.000000000 and 23:59:59.999999999. Value: {value}");
        }

        return microseconds;
    }

    public static long? TryTimeOnlyToMicroseconds(TimeOnly? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeOnlyToMicroseconds(value.Value));

    public static TimeOnly TimeOnlyFromMicroseconds(long value)
    {
        if (value < 0 || value >= DateTimeAlgorithms.MicrosecondsPerDay)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"The TimeOnly value must be between 0 and {DateTimeAlgorithms.MicrosecondsPerDay - 1}. Value: {value}");
        }

        return TimeOnly.FromTimeSpan(TimeSpan.FromTicks(TicksFromMicroseconds(value)));
    }

    public static TimeOnly? TryTimeOnlyFromMicroseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeOnlyFromMicroseconds(value.Value));

    public static long TimeOnlyToMilliseconds(TimeOnly value)
    {
        var milliseconds = TicksToMilliseconds(value.Ticks);

        if (milliseconds < 0 || milliseconds >= DateTimeAlgorithms.MillisecondsPerDay)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"The TimeOnly value must be between 00:00:00.000000000 and 23:59:59.999999999. Value: {value}");
        }

        return milliseconds;
    }

    public static long? TryTimeOnlyToMilliseconds(TimeOnly? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeOnlyToMilliseconds(value.Value));

    public static TimeOnly TimeOnlyFromMilliseconds(long value)
    {
        if (value < 0 || value >= DateTimeAlgorithms.MillisecondsPerDay)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"The TimeOnly value must be between 0 and {DateTimeAlgorithms.MillisecondsPerDay - 1}. Value: {value}");
        }

        return TimeOnly.FromTimeSpan(TimeSpan.FromTicks(TicksFromMilliseconds(value)));
    }

    public static TimeOnly? TryTimeOnlyFromMilliseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeOnlyFromMilliseconds(value.Value));

    public const long TimeOnlySerialWithNanosecondsHourMultiplier = 10000000000000;
    public const long TimeOnlySerialWithNanosecondsMinuteMultiplier = 100000000000;
    public const long TimeOnlySerialWithNanosecondsSecondMultiplier = 1000000000;

    public static long TimeOnlyToSerialWithNanoseconds(TimeOnly value)
    {
        long nanoseconds = TimeOnlyToNanoseconds(value);

        return (value.Hour * TimeOnlySerialWithNanosecondsHourMultiplier)
             + (value.Minute * TimeOnlySerialWithNanosecondsMinuteMultiplier)
             + (value.Second * TimeOnlySerialWithNanosecondsSecondMultiplier)
             + nanoseconds % TimeOnlySerialWithNanosecondsSecondMultiplier;
    }

    public static long? TryTimeOnlyToSerialWithNanoseconds(TimeOnly? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeOnlyToSerialWithNanoseconds(value.Value));

    public static TimeOnly TimeOnlyFromSerialWithNanoseconds(long value)
    {
        CheckSerialDateOrTimeValue(value);

        int hour = (int)(value / TimeOnlySerialWithNanosecondsHourMultiplier);
        int minute = (int)((value % TimeOnlySerialWithNanosecondsHourMultiplier) / TimeOnlySerialWithNanosecondsMinuteMultiplier);
        int second = (int)((value % TimeOnlySerialWithNanosecondsMinuteMultiplier) / TimeOnlySerialWithNanosecondsSecondMultiplier);
        long nanoseconds = value % TimeOnlySerialWithNanosecondsSecondMultiplier;

        return TimeOnly.FromTimeSpan(new TimeOnly(hour, minute, second).ToTimeSpan() + TimeSpanFromNanoseconds(nanoseconds));
    }

    public static TimeOnly? TryTimeOnlyFromSerialWithNanoseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeOnlyFromSerialWithNanoseconds(value.Value));

    public const long TimeOnlySerialWithMicrosecondsHourMultiplier = TimeOnlySerialWithNanosecondsHourMultiplier / 1000;
    public const long TimeOnlySerialWithMicrosecondsMinuteMultiplier = TimeOnlySerialWithNanosecondsMinuteMultiplier / 1000;
    public const long TimeOnlySerialWithMicrosecondsSecondMultiplier = TimeOnlySerialWithNanosecondsSecondMultiplier / 1000;

    public static long TimeOnlyToSerialWithMicroseconds(TimeOnly value)
    {
        long microseconds = TimeOnlyToMicroseconds(value);

        return (value.Hour * TimeOnlySerialWithMicrosecondsHourMultiplier)
             + (value.Minute * TimeOnlySerialWithMicrosecondsMinuteMultiplier)
             + (value.Second * TimeOnlySerialWithMicrosecondsSecondMultiplier)
             + microseconds % TimeOnlySerialWithMicrosecondsSecondMultiplier;
    }

    public static long? TryTimeOnlyToSerialWithMicroseconds(TimeOnly? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeOnlyToSerialWithMicroseconds(value.Value));

    public static TimeOnly TimeOnlyFromSerialWithMicroseconds(long value)
    {
        CheckSerialDateOrTimeValue(value);

        int hour = (int)(value / TimeOnlySerialWithMicrosecondsHourMultiplier);
        int minute = (int)((value % TimeOnlySerialWithMicrosecondsHourMultiplier) / TimeOnlySerialWithMicrosecondsMinuteMultiplier);
        int second = (int)((value % TimeOnlySerialWithMicrosecondsMinuteMultiplier) / TimeOnlySerialWithMicrosecondsSecondMultiplier);
        long microseconds = value % TimeOnlySerialWithMicrosecondsSecondMultiplier;

        return TimeOnly.FromTimeSpan(new TimeOnly(hour, minute, second).ToTimeSpan() + TimeSpanFromMicroseconds(microseconds));
    }

    public static TimeOnly? TryTimeOnlyFromSerialWithMicroseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeOnlyFromSerialWithMicroseconds(value.Value));

    public const long TimeOnlySerialWithMillisecondsHourMultiplier = TimeOnlySerialWithMicrosecondsHourMultiplier / 1000;
    public const long TimeOnlySerialWithMillisecondsMinuteMultiplier = TimeOnlySerialWithMicrosecondsMinuteMultiplier / 1000;
    public const long TimeOnlySerialWithMillisecondsSecondMultiplier = TimeOnlySerialWithMicrosecondsSecondMultiplier / 1000;

    public static long TimeOnlyToSerialWithMilliseconds(TimeOnly value)
    {
        long milliseconds = TimeOnlyToMilliseconds(value);

        return (value.Hour * TimeOnlySerialWithMillisecondsHourMultiplier)
             + (value.Minute * TimeOnlySerialWithMillisecondsMinuteMultiplier)
             + (value.Second * TimeOnlySerialWithMillisecondsSecondMultiplier)
             + milliseconds % TimeOnlySerialWithMillisecondsSecondMultiplier;
    }

    public static long? TryTimeOnlyToSerialWithMilliseconds(TimeOnly? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeOnlyToSerialWithMilliseconds(value.Value));

    public static TimeOnly TimeOnlyFromSerialWithMilliseconds(long value)
    {
        CheckSerialDateOrTimeValue(value);

        int hour = (int)(value / TimeOnlySerialWithMillisecondsHourMultiplier);
        int minute = (int)((value % TimeOnlySerialWithMillisecondsHourMultiplier) / TimeOnlySerialWithMillisecondsMinuteMultiplier);
        int second = (int)((value % TimeOnlySerialWithMillisecondsMinuteMultiplier) / TimeOnlySerialWithMillisecondsSecondMultiplier);
        long milliseconds = value % TimeOnlySerialWithMillisecondsSecondMultiplier;

        return TimeOnly.FromTimeSpan(new TimeOnly(hour, minute, second).ToTimeSpan() + TimeSpanFromMilliseconds(milliseconds));
    }

    public static TimeOnly? TryTimeOnlyFromSerialWithMilliseconds(long? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeOnlyFromSerialWithMilliseconds(value.Value));

    public const int TimeOnlySerialHourMultiplier = 10000;
    public const int TimeOnlySerialMinuteMultiplier = 100;
    public const int TimeOnlySerialSecondMultiplier = 1;

    public static int TimeOnlyToSerial(TimeOnly value)
        => (value.Hour * TimeOnlySerialHourMultiplier)
            + (value.Minute * TimeOnlySerialMinuteMultiplier)
            + value.Second;

    public static int? TryTimeOnlyToSerial(TimeOnly? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeOnlyToSerial(value.Value));

    public static TimeOnly TimeOnlyFromSerial(int value)
    {
        CheckSerialDateOrTimeValue(value);

        int hour = value / TimeOnlySerialHourMultiplier;
        int minute = (value % TimeOnlySerialHourMultiplier) / TimeOnlySerialMinuteMultiplier;
        int second = value % TimeOnlySerialMinuteMultiplier;

        return new TimeOnly(hour, minute, second);
    }

    public static TimeOnly? TryTimeOnlyFromSerial(int? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => TimeOnlyFromSerial(value.Value));

    public static string GuidToString(Guid value, CultureInfo? cultureInfo = null)
        => value.ToString("D", cultureInfo ?? CultureInfo.InvariantCulture);

    public static string? TryGuidToString(Guid? value, CultureInfo? cultureInfo = null)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullReference(() => GuidToString(value.Value, cultureInfo));

    public static Guid GuidFromString(string value, CultureInfo? cultureInfo = null)
        => Guid.Parse(value, cultureInfo ?? CultureInfo.InvariantCulture);

    public static Guid? TryGuidFromString(string? value, CultureInfo? cultureInfo = null)
        => string.IsNullOrWhiteSpace(value) ? null : Guid.TryParse(value, cultureInfo ?? CultureInfo.InvariantCulture, out var result) ? result : null;

    public static string GuidToCompactString(Guid value, CultureInfo? cultureInfo = null)
        => value.ToString("N", cultureInfo ?? CultureInfo.InvariantCulture);

    public static string? TryGuidToCompactString(Guid? value, CultureInfo? cultureInfo = null)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullReference(() => GuidToCompactString(value.Value, cultureInfo));

    public static Guid GuidFromCompactString(string value)
        => Guid.ParseExact(value, "N");

    public static Guid? TryGuidFromCompactString(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : Guid.TryParseExact(value, "N", out var result) ? result : null;

    public static byte[] GuidToBigEndianByteArray(Guid value)
        => Convert.FromHexString(GuidToCompactString(value));

    public static byte[]? TryGuidToBigEndianByteArray(Guid? value)
        => value == null ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullReference(() => GuidToBigEndianByteArray(value.Value));

    public static Guid GuidFromBigEndianByteArray(byte[] value)
        => GuidFromCompactString(ByteArrayToBase16String(value));

    public static Guid? TryGuidFromBigEndianByteArray(byte[]? value)
        => (value == null) || (value.Length == 0) ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullValue(() => GuidFromCompactString(ByteArrayToBase16String(value)));

    public static string ByteArrayToBase64String(byte[] value)
        => Convert.ToBase64String(value);

    public static string? TryByteArrayToBase64String(byte[]? value)
        => (value == null) || (value.Length == 0) ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullReference(() => ByteArrayToBase64String(value));

    public static byte[] ByteArrayFromBase64String(string value)
        => Convert.FromBase64String(value);

    public static byte[]? TryByteArrayFromBase64String(string? value)
        => (value == null) || (value.Length == 0) ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullReference(() => ByteArrayFromBase64String(value));

    public static string ByteArrayToBase16String(byte[] value)
        => Convert.ToHexString(value);

    public static string? TryByteArrayToBase16String(byte[]? value)
        => (value == null) || (value.Length == 0) ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullReference(() => ByteArrayToBase16String(value));

    public static byte[] ByteArrayFromBase16String(string value)
        => Convert.FromHexString(value);

    public static byte[]? TryByteArrayFromBase16String(string? value)
        => (value == null) || (value.Length == 0) ? null : ExceptionProtector.ExecuteFunctionAndReturnOrNullReference(() => ByteArrayFromBase16String(value));
}
