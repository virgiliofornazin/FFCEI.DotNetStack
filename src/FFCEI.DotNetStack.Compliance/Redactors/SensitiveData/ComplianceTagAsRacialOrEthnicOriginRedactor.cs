using FFCEI.DotNetStack.Compliance.Common.Redactors;

namespace FFCEI.DotNetStack.Compliance.Redactors.SensitiveData;

public sealed class ComplianceTagAsRacialOrEthnicOriginRedactor()
    : ReplaceRedactor("***")
{
}
