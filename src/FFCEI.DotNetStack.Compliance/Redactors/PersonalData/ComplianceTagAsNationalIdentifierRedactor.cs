using FFCEI.DotNetStack.Compliance.Common.Redactors;

namespace FFCEI.DotNetStack.Compliance.Redactors.PersonalData;

public class ComplianceTagAsNationalIdentifierRedactor()
    : MaskingRedactor(showFirst: 0, showLast: 4, maskChar: '*', preserveLength: true)
{
}
