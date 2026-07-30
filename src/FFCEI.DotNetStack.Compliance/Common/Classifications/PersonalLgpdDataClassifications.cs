using Microsoft.Extensions.Compliance.Classification;

namespace FFCEI.DotNetStack.Compliance.Common.Classifications;

public static class PersonalLgpdDataClassifications
{
    private const string TaxonomyName = "PersonalData.LGPD";

    public static DataClassification Cin { get; } = new(TaxonomyName, nameof(Cin));
    public static DataClassification Cpf { get; } = new(TaxonomyName, nameof(Cpf));
    public static DataClassification Rg { get; } = new(TaxonomyName, nameof(Rg));
}
