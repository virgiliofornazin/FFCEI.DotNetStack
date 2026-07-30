using FFCEI.DotNetStack.Compliance.Common.Redactors;

namespace FFCEI.DotNetStack.Compliance.Redactors.PersonalData;

public class ComplianceTagAsDriverLicenseNumberRedactor()
    : MaskingRedactor(showFirst: 0, showLast: 2, maskChar: '*', preserveLength: true)
{
}
