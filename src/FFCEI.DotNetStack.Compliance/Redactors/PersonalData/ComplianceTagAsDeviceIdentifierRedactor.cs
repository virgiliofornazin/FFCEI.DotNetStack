using FFCEI.DotNetStack.Compliance.Common.Redactors;

namespace FFCEI.DotNetStack.Compliance.Redactors.PersonalData;

public sealed class ComplianceTagAsDeviceIdentifierRedactor()
    : MaskingRedactor(showFirst: 0, showLast: 4, maskChar: '*', preserveLength: true)
{
}
