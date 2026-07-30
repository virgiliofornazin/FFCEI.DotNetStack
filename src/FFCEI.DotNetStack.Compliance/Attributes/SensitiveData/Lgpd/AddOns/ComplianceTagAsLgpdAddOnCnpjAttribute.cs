using FFCEI.DotNetStack.Compliance.Common.Attributes;
using FFCEI.DotNetStack.Compliance.Common.Classifications;
using FFCEI.DotNetStack.Compliance.Redactors.SensitiveData.Lgpd.AddOns;

namespace FFCEI.DotNetStack.Compliance.Attributes.SensitiveData.Lgpd.AddOns;

public sealed class ComplianceTagAsLgpdAddOnCnpjAttribute()
    : MaskingRedactorAttribute<ComplianceTagAsLgpdAddOnCnpjRedactor>(SensitiveLgpdAddOnDataClassifications.Cnpj)
{
}
