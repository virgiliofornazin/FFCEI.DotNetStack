using Microsoft.Extensions.Compliance.Classification;

namespace FFCEI.DotNetStack.Compliance.Common.Classifications;

public static class SensitiveAppiDataClassifications
{
    private const string TaxonomyName = "SensitiveData.APPI";

    public static DataClassification CrimeVictimHistory { get; } = new(TaxonomyName, nameof(CrimeVictimHistory));
    public static DataClassification PhysicalOrMentalDisability { get; } = new(TaxonomyName, nameof(PhysicalOrMentalDisability));
    public static DataClassification SocialStatus { get; } = new(TaxonomyName, nameof(SocialStatus));
}
