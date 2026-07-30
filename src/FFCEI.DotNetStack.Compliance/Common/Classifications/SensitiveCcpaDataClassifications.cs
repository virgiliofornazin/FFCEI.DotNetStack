using Microsoft.Extensions.Compliance.Classification;

namespace FFCEI.DotNetStack.Compliance.Common.Classifications;

public static class SensitiveCcpaDataClassifications
{
    private const string TaxonomyName = "SensitiveData.CCPA";

    public static DataClassification CitizenshipOrImmigrationStatus { get; } = new(TaxonomyName, nameof(CitizenshipOrImmigrationStatus));
    public static DataClassification CommunicationsContent { get; } = new(TaxonomyName, nameof(CommunicationsContent));
}
