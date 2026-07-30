namespace FFCEI.DotNetStack.Framework;

[Flags]
public enum RandomStringGeneratorSubsets
{
    None = 0,
    Digits = 1,
    HexadecimalDigitsUppercase = 2,
    HexadecimalDigitsLowercase = 4,
    AsciiUppercase = 8,
    AsciiLowercase = 16,
}
