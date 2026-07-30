using Microsoft.Extensions.Compliance.Classification;

namespace FFCEI.DotNetStack.Compliance.Common.Classifications;

public static class PersonalCcpaDataClassifications
{
    private const string TaxonomyName = "PersonalData.CCPA";

    public static DataClassification SocialSecurityNumber { get; } = new(TaxonomyName, nameof(SocialSecurityNumber));
    public static DataClassification StateIdentificationCardNumber { get; } = new(TaxonomyName, nameof(StateIdentificationCardNumber));
}
