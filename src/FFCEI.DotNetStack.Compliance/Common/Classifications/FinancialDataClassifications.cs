using Microsoft.Extensions.Compliance.Classification;

namespace FFCEI.DotNetStack.Compliance.Common.Classifications;

public static class FinancialDataClassifications
{
    private const string TaxonomyName = "FinancialData";

    public static DataClassification BankAccountNumber { get; } = new(TaxonomyName, nameof(BankAccountNumber));
    public static DataClassification CreditCardNumber { get; } = new(TaxonomyName, nameof(CreditCardNumber));
    public static DataClassification TransferAddress { get; } = new(TaxonomyName, nameof(TransferAddress));
}
