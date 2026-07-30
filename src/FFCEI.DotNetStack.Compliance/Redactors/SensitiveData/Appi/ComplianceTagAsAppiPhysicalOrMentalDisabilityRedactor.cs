using FFCEI.DotNetStack.Compliance.Common.Redactors;

namespace FFCEI.DotNetStack.Compliance.Redactors.SensitiveData.Appi;

public sealed class ComplianceTagAsAppiPhysicalOrMentalDisabilityRedactor()
    : ReplaceRedactor("***")
{
}
