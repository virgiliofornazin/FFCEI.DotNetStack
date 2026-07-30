using Microsoft.Extensions.Compliance.Classification;

namespace FFCEI.DotNetStack.Compliance.Common.Classifications;

public static class PersonalAppiDataClassifications
{
    private const string TaxonomyName = "PersonalData.APPI";

    public static DataClassification BasicResidentRegisterCode { get; } = new(TaxonomyName, nameof(BasicResidentRegisterCode));
    public static DataClassification MyNumber { get; } = new(TaxonomyName, nameof(MyNumber));
    public static DataClassification PensionNumber { get; } = new(TaxonomyName, nameof(PensionNumber));
}
