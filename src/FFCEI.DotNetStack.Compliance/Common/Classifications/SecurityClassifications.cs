using Microsoft.Extensions.Compliance.Classification;

namespace FFCEI.DotNetStack.Compliance.Common.Classifications;

public static class SecurityClassifications
{
    private const string TaxonomyName = "Security";

    public static DataClassification ApiKey { get; } = new(TaxonomyName, nameof(ApiKey));
    public static DataClassification Password { get; } = new(TaxonomyName, nameof(Password));
    public static DataClassification Secret { get; } = new(TaxonomyName, nameof(Secret));
    public static DataClassification VerificationCode { get; } = new(TaxonomyName, nameof(VerificationCode));
}
