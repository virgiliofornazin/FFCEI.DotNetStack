using FFCEI.DotNetStack.Compliance.Common.Redactors;

namespace FFCEI.DotNetStack.Compliance.Redactors.SensitiveData.Lgpd;

public sealed class ComplianceTagAsLgpdOrganizationMembershipRedactor()
    : ReplaceRedactor("***")
{
}
