using System.Security.Cryptography;

namespace FFCEI.DotNetStack.Framework;

public sealed class RandomStringGenerator
{
    public const string SubsetOfDigits = "0123456789";
    public const string SubsetOfHexadecimalDigitsUppercase = $"ABCDEF";
    public const string SubsetOfHexadecimalDigitsLowercase = $"abcdef";
    public const string SubsetOfAsciiUppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string SubsetOfAsciiLowercase = "abcdefghijklmnopqrstuvwxyz";

    public static string GenerateWith(MultipleStringsReadOnlySpan span, int length)
    {
        if (span.Length < 1)
        {
            throw new InvalidOperationException("No source characters to generate random value");
        }

        var result = new char[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = span[RandomNumberGenerator.GetInt32(span.Length)];
        }

        return new string(result);
    }

    public static string GenerateWithSubsets(RandomStringGeneratorSubsets subset, int length)
    {
        using var span = new MultipleStringsReadOnlySpan();

        if (subset.HasFlag(RandomStringGeneratorSubsets.HexadecimalDigitsUppercase))
        {
            if (!span.Any(c => c == '0'))
            {
                span.Add(SubsetOfDigits);
            }

            span.Add(SubsetOfHexadecimalDigitsUppercase);
        }

        if (subset.HasFlag(RandomStringGeneratorSubsets.HexadecimalDigitsLowercase))
        {
            if (!span.Any(c => c == '0'))
            {
                span.Add(SubsetOfDigits);
            }

            span.Add(SubsetOfHexadecimalDigitsLowercase);
        }

        if (subset.HasFlag(RandomStringGeneratorSubsets.Digits))
        {
            if (!span.Any(c => c == '0'))
            {
                span.Add(SubsetOfDigits);
            }
        }

        if (subset.HasFlag(RandomStringGeneratorSubsets.AsciiUppercase))
        {
            span.Add(SubsetOfAsciiUppercase);
        }

        if (subset.HasFlag(RandomStringGeneratorSubsets.AsciiLowercase))
        {
            span.Add(SubsetOfAsciiLowercase);
        }

        return GenerateWith(span, length);
    }

    public static string GenerateWithCustomSubset(Span<char> customSubset, int length)
    {
        using var span = new MultipleStringsReadOnlySpan();

        span.Add(customSubset);

        return GenerateWith(span, length);
    }
}
