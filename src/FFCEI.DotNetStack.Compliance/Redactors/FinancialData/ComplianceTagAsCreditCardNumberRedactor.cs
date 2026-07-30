using FFCEI.DotNetStack.Compliance.Common.Redactors;

namespace FFCEI.DotNetStack.Compliance.Redactors.FinancialData;

public sealed class ComplianceTagAsCreditCardNumberRedactor()
    : MaskingRedactor(showFirst: 3, showLast: 4, maskChar: '*', preserveLength: true)
{
}
