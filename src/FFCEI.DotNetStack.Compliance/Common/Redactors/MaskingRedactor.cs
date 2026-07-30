using Microsoft.Extensions.Compliance.Redaction;

namespace FFCEI.DotNetStack.Compliance.Common.Redactors;

public abstract class MaskingRedactor(int showFirst = 0, int showLast = 0, char maskChar = '*', bool preserveLength = true)
    : Redactor
{
    public int ShowFirst { get; } = showFirst;
    public int ShowLast { get; } = showLast;
    public char MaskChar { get; } = maskChar;
    public bool PreserveLength { get; } = preserveLength;

    public override int GetRedactedLength(ReadOnlySpan<char> input)
        => PreserveLength ? input.Length : Math.Min(input.Length, ShowFirst + ShowLast + 3);

    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        var visibleAtStart = Math.Min(ShowFirst, source.Length);
        var visibleAtEnd = Math.Min(ShowLast, source.Length - visibleAtStart);

        if (visibleAtStart + visibleAtEnd >= source.Length)
        {
            visibleAtStart = 0;
            visibleAtEnd = 0;
        }

        var maskedLength = GetRedactedLength(source);

        source[..visibleAtStart].CopyTo(destination);
        source[(source.Length - visibleAtEnd)..].CopyTo(destination[(maskedLength - visibleAtEnd)..]);

        for (var i = visibleAtStart; i < maskedLength - visibleAtEnd; i++)
        {
            destination[i] = MaskChar;
        }

        return maskedLength;
    }
}
