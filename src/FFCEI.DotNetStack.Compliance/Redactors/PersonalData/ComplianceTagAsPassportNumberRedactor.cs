using FFCEI.DotNetStack.Compliance.Common.Redactors;

namespace FFCEI.DotNetStack.Compliance.Redactors.PersonalData;

public sealed class ComplianceTagAsPassportNumberRedactor()
    : MaskingRedactor(showFirst: 0, showLast: 2, maskChar: '*', preserveLength: true)
{
}
