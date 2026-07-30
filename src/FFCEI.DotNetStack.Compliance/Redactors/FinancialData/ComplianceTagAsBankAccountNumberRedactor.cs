using FFCEI.DotNetStack.Compliance.Common.Redactors;

namespace FFCEI.DotNetStack.Compliance.Redactors.FinancialData;

public sealed class ComplianceTagAsBankAccountNumberRedactor()
    : ReplaceRedactor("***")
{
}
