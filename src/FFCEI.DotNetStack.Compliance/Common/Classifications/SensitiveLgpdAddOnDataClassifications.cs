using Microsoft.Extensions.Compliance.Classification;

namespace FFCEI.DotNetStack.Compliance.Common.Classifications;

public static class SensitiveLgpdAddOnDataClassifications
{
    private const string TaxonomyName = "SensitiveData.LGPD.AddOn";

    public static DataClassification Cnpj { get; } = new(TaxonomyName, nameof(Cnpj));

    public static DataClassification StateTaxId { get; } = new(TaxonomyName, nameof(StateTaxId));

    public static DataClassification MunicipalTaxId { get; } = new(TaxonomyName, nameof(MunicipalTaxId));
}
