using Microsoft.Extensions.Compliance.Classification;

namespace FFCEI.DotNetStack.Compliance.Common.Classifications;

public static class InformationSecurityIso27001Classifications
{
    private const string TaxonomyName = "InformationSecurity.ISO27001";

    public static DataClassification ConfidentialInformation { get; } = new(TaxonomyName, nameof(ConfidentialInformation));
    public static DataClassification InternalInformation { get; } = new(TaxonomyName, nameof(InternalInformation));
    public static DataClassification PublicInformation { get; } = new(TaxonomyName, nameof(PublicInformation));
    public static DataClassification RestrictedInformation { get; } = new(TaxonomyName, nameof(RestrictedInformation));
}
