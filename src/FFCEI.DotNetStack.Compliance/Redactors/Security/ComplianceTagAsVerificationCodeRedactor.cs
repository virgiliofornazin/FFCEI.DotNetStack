using FFCEI.DotNetStack.Compliance.Common.Redactors;

namespace FFCEI.DotNetStack.Compliance.Redactors.Security;

public sealed class ComplianceTagAsVerificationCodeRedactor()
    : ReplaceRedactor("***")
{
}
