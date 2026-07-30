using FFCEI.DotNetStack.Compliance.Common.Attributes;
using FFCEI.DotNetStack.Compliance.Common.Classifications;
using FFCEI.DotNetStack.Compliance.Redactors.FinancialData;

namespace FFCEI.DotNetStack.Compliance.Attributes.FinancialData;

public sealed class ComplianceTagAsBankAccountNumberAttribute()
    : ReplaceRedactorAttribute<ComplianceTagAsBankAccountNumberRedactor>(FinancialDataClassifications.BankAccountNumber)
{
}
