using Microsoft.Extensions.Compliance.Classification;

namespace FFCEI.DotNetStack.Compliance.Common.Classifications;

public static class PersonalPipedaDataClassifications
{
    private const string TaxonomyName = "PersonalData.PIPEDA";

    public static DataClassification SocialInsuranceNumber { get; } = new(TaxonomyName, nameof(SocialInsuranceNumber));
}
