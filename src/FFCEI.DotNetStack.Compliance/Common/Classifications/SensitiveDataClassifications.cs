using Microsoft.Extensions.Compliance.Classification;

namespace FFCEI.DotNetStack.Compliance.Common.Classifications;

public static class SensitiveDataClassifications
{
    private const string TaxonomyName = "SensitiveData";

    public static DataClassification BiometricData { get; } = new(TaxonomyName, nameof(BiometricData));
    public static DataClassification CriminalRecord { get; } = new(TaxonomyName, nameof(CriminalRecord));
    public static DataClassification GeneticData { get; } = new(TaxonomyName, nameof(GeneticData));
    public static DataClassification HealthData { get; } = new(TaxonomyName, nameof(HealthData));
    public static DataClassification HealthInsuranceNumber { get; } = new(TaxonomyName, nameof(HealthInsuranceNumber));
    public static DataClassification PoliticalOpinion { get; } = new(TaxonomyName, nameof(PoliticalOpinion));
    public static DataClassification PreciseGeolocation { get; } = new(TaxonomyName, nameof(PreciseGeolocation));
    public static DataClassification RacialOrEthnicOrigin { get; } = new(TaxonomyName, nameof(RacialOrEthnicOrigin));
    public static DataClassification ReligiousOrPhilosophicalBelief { get; } = new(TaxonomyName, nameof(ReligiousOrPhilosophicalBelief));
    public static DataClassification SexualLifeOrOrientationData { get; } = new(TaxonomyName, nameof(SexualLifeOrOrientationData));
    public static DataClassification TradeUnionMembership { get; } = new(TaxonomyName, nameof(TradeUnionMembership));
}
