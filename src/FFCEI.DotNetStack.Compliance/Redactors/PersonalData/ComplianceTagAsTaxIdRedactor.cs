using FFCEI.DotNetStack.Compliance.Common.Redactors;

namespace FFCEI.DotNetStack.Compliance.Redactors.PersonalData;

public class ComplianceTagAsTaxIdRedactor()
    : MaskingRedactor(showFirst: 4, showLast: 0, maskChar: '*', preserveLength: true)
{
}
