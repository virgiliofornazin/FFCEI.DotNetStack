using FFCEI.DotNetStack.Compliance.Common.Redactors;

namespace FFCEI.DotNetStack.Compliance.Redactors.PersonalData;

public sealed class ComplianceTagAsAddressRedactor()
    : ReplaceRedactor("***")
{
}
