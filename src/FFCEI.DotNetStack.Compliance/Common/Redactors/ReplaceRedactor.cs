using Microsoft.Extensions.Compliance.Redaction;

namespace FFCEI.DotNetStack.Compliance.Common.Redactors;

public abstract class ReplaceRedactor(string replacementText)
    : Redactor
{
    public string ReplacementText { get; } = replacementText;

    public override int GetRedactedLength(ReadOnlySpan<char> input)
        => ReplacementText.Length;

    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        ReplacementText.CopyTo(destination);

        return ReplacementText.Length;
    }
}
