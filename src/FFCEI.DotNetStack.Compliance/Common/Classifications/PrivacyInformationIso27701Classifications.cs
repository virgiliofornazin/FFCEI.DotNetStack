using Microsoft.Extensions.Compliance.Classification;

namespace FFCEI.DotNetStack.Compliance.Common.Classifications;

public static class PrivacyInformationIso27701Classifications
{
    private const string TaxonomyName = "PrivacyInformation.ISO27701";

    public static DataClassification Pii { get; } = new(TaxonomyName, nameof(Pii));
    public static DataClassification PiiPrincipalIdentifier { get; } = new(TaxonomyName, nameof(PiiPrincipalIdentifier));
    public static DataClassification PseudonymizedPii { get; } = new(TaxonomyName, nameof(PseudonymizedPii));
    public static DataClassification SensitivePii { get; } = new(TaxonomyName, nameof(SensitivePii));
}
