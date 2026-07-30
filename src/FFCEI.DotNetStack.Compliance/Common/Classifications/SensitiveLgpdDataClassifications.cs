using Microsoft.Extensions.Compliance.Classification;

namespace FFCEI.DotNetStack.Compliance.Common.Classifications;

public static class SensitiveLgpdDataClassifications
{
    private const string TaxonomyName = "SensitiveData.LGPD";

    public static DataClassification OrganizationMembership { get; } = new(TaxonomyName, nameof(OrganizationMembership));
}
